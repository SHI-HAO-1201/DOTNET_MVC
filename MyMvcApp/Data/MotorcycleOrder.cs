namespace MyMvcApp.Data;

public sealed class MotorcycleOrder
{
    public int Id { get; set; }
    public required string OrderNumber { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public long TotalAmount { get; set; }
    public long DiscountAmount { get; set; }
    public string? PromotionCode { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public required string PaymentMethod { get; set; }
    public string Status { get; set; } = "Pending";
    public ICollection<MotorcycleOrderItem> Items { get; set; } = new List<MotorcycleOrderItem>();
}
