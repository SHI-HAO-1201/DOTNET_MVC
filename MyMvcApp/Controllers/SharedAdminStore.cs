using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace MyMvcApp.Services;

public sealed class SharedAdminStore(IConfiguration configuration)
{
    private readonly PasswordHasher<object> _hasher = new();

    public async Task<(int Id, string Name, string Email)?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        email = NormalizeEmail(email);
        await using var connection = new SqlConnection(GetConnectionString()); await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("SELECT TOP (1) u.Id,u.HoTen,u.Email,u.PasswordHash FROM dbo.AspNetUsers u INNER JOIN dbo.AspNetUserRoles ur ON ur.UserId=u.Id INNER JOIN dbo.AspNetRoles r ON r.Id=ur.RoleId WHERE u.NormalizedEmail=@email AND u.DangHoatDong=1 AND r.Name IN (N'Admin',N'QuanTriVien')", connection);
        command.Parameters.AddWithValue("@email", email.ToUpperInvariant());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var hash = reader.IsDBNull(3) ? null : reader.GetString(3);
        if (string.IsNullOrWhiteSpace(hash) || _hasher.VerifyHashedPassword(new object(), hash, password) == PasswordVerificationResult.Failed) return null;
        return (reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
    }

    public async Task<int> CreateOrResetAsync(string? name, string? email, string? phone, string? password, CancellationToken cancellationToken = default)
    {
        name = (name ?? string.Empty).Trim(); email = NormalizeEmail(email); phone = (phone ?? string.Empty).Trim();
        if (name.Length is < 2 or > 100 || !new EmailAddressAttribute().IsValid(email) || phone.Length > 30 || password is null || password.Length is < 8 or > 256) throw new ArgumentException("Tên, email, số điện thoại hoặc mật khẩu không hợp lệ.");
        var hash = _hasher.HashPassword(new object(), password);
        await using var connection = new SqlConnection(GetConnectionString()); await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var userId = await Scalar(connection, transaction, "SELECT TOP (1) Id FROM dbo.AspNetUsers WITH (UPDLOCK,HOLDLOCK) WHERE NormalizedEmail=@email", cancellationToken, ("@email", email.ToUpperInvariant()));
        if (userId is null)
            userId = await Scalar(connection, transaction, "INSERT dbo.AspNetUsers (HoTen,DangHoatDong,ChiNhanhId,NgayTao,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnabled,AccessFailedCount) OUTPUT INSERTED.Id VALUES (@name,1,NULL,SYSUTCDATETIME(),@email,@normalized,@email,@normalized,1,@hash,@stamp,@stamp,@phone,0,0,1,0)", cancellationToken, ("@name", name), ("@email", email), ("@normalized", email.ToUpperInvariant()), ("@hash", hash), ("@stamp", Guid.NewGuid().ToString("N")), ("@phone", phone));
        else await Execute(connection, transaction, "UPDATE dbo.AspNetUsers SET HoTen=@name,DangHoatDong=1,PasswordHash=@hash,SecurityStamp=@stamp,ConcurrencyStamp=@stamp,PhoneNumber=@phone,EmailConfirmed=1,AccessFailedCount=0,LockoutEnd=NULL WHERE Id=@id", cancellationToken, ("@name", name), ("@hash", hash), ("@stamp", Guid.NewGuid().ToString("N")), ("@phone", phone), ("@id", Convert.ToInt32(userId)));
        var id = Convert.ToInt32(userId);
        foreach (var role in new[] { "Admin", "QuanTriVien" })
        {
            var roleId = await Scalar(connection, transaction, "SELECT TOP (1) Id FROM dbo.AspNetRoles WHERE NormalizedName=@name", cancellationToken, ("@name", role.ToUpperInvariant()));
            if (roleId is null) roleId = await Scalar(connection, transaction, "INSERT dbo.AspNetRoles (Name,NormalizedName,ConcurrencyStamp) OUTPUT INSERTED.Id VALUES (@name,@normalized,@stamp)", cancellationToken, ("@name", role), ("@normalized", role.ToUpperInvariant()), ("@stamp", Guid.NewGuid().ToString("N")));
            await Execute(connection, transaction, "IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId=@user AND RoleId=@role) INSERT dbo.AspNetUserRoles (UserId,RoleId) VALUES (@user,@role)", cancellationToken, ("@user", id), ("@role", Convert.ToInt32(roleId)));
        }
        await transaction.CommitAsync(cancellationToken); return id;
    }

    private string GetConnectionString() => configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Chưa cấu hình ConnectionStrings:DefaultConnection.");
    private static string NormalizeEmail(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
    private static async Task<object?> Scalar(SqlConnection c, SqlTransaction t, string sql, CancellationToken ct, params (string, object)[] ps) { await using var cmd=new SqlCommand(sql,c,t); foreach(var p in ps)cmd.Parameters.AddWithValue(p.Item1,p.Item2); return await cmd.ExecuteScalarAsync(ct); }
    private static async Task Execute(SqlConnection c, SqlTransaction t, string sql, CancellationToken ct, params (string, object)[] ps) { await using var cmd=new SqlCommand(sql,c,t); foreach(var p in ps)cmd.Parameters.AddWithValue(p.Item1,p.Item2); await cmd.ExecuteNonQueryAsync(ct); }
}

public static class AdminAccountCommand
{
    public static async Task<int> RunAsync(SharedAdminStore store)
    {
        Console.OutputEncoding=System.Text.Encoding.UTF8; Console.Write("Họ và tên: "); var name=Console.ReadLine(); Console.Write("Email đăng nhập: "); var email=Console.ReadLine(); Console.Write("Số điện thoại: "); var phone=Console.ReadLine();
        var password=ReadPassword("Mật khẩu (ít nhất 8 ký tự): "); if(password!=ReadPassword("Nhập lại mật khẩu: ")){Console.Error.WriteLine("Mật khẩu nhập lại không khớp.");return 1;}
        try{var id=await store.CreateOrResetAsync(name,email,phone,password);Console.WriteLine($"Đã tạo/cập nhật admin dùng chung ID {id} ({email?.Trim()}).");return 0;}catch(Exception ex){Console.Error.WriteLine($"Không thể tạo admin: {ex.Message}");return 1;}
    }
    private static string ReadPassword(string prompt){Console.Write(prompt);var value=new System.Text.StringBuilder();while(true){var key=Console.ReadKey(true);if(key.Key==ConsoleKey.Enter){Console.WriteLine();return value.ToString();}if(key.Key==ConsoleKey.Backspace){if(value.Length>0)value.Length--;}else if(!char.IsControl(key.KeyChar))value.Append(key.KeyChar);}}
}
