namespace MyMvcApp.Data;

public sealed class MotorcycleReview
{
    public int Id { get; set; }
    public int MotorcycleId { get; set; }
    public Motorcycle Motorcycle { get; set; } = null!;
    public required string ReviewerName { get; set; }
    public required string ReviewerEmail { get; set; }
    public int Rating { get; set; }
    public required string Comment { get; set; }
    public string? AdminReply { get; set; }
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
