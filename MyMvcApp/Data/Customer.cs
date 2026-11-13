namespace MyMvcApp.Data;

public sealed class Customer
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public string? PasswordHash { get; set; }
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<MotorcycleOrder> Orders { get; set; } = new List<MotorcycleOrder>();
}
