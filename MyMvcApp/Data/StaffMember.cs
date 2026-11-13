namespace MyMvcApp.Data;

public sealed class StaffMember
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public string StoreLocation { get; set; } = "Showroom chính";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
