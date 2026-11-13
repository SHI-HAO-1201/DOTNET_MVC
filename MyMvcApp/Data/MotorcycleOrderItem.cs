namespace MyMvcApp.Data;

public sealed class MotorcycleOrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public MotorcycleOrder Order { get; set; } = null!;
    public int MotorcycleId { get; set; }
    public required string MotorcycleName { get; set; }
    public long UnitPrice { get; set; }
    public int Quantity { get; set; }
}
