namespace MyMvcApp.Data;

public sealed class Promotion
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string Description { get; set; } = string.Empty;
    public long DiscountAmount { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
