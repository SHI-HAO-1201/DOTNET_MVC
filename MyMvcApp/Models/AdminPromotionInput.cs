using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models;

public sealed class AdminPromotionInput
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(32)] public string Code { get; set; } = string.Empty;
    [StringLength(300)] public string Description { get; set; } = string.Empty;
    [Range(0, long.MaxValue)] public long DiscountAmount { get; set; }
    [Range(0, int.MaxValue)] public int UsageLimit { get; set; }
    [DataType(DataType.Date)] public DateTime StartsAt { get; set; } = DateTime.Today;
    [DataType(DataType.Date)] public DateTime EndsAt { get; set; } = DateTime.Today.AddMonths(1);
    public bool IsActive { get; set; } = true;
}
