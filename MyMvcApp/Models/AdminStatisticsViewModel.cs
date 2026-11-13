using MyMvcApp.Data;

namespace MyMvcApp.Models;

/// <summary>
/// Centralized admin metrics used by the admin dashboard.
/// </summary>
public sealed class AdminStatisticsViewModel
{
    public IReadOnlyList<decimal> MonthlyRevenue { get; init; } = new decimal[12];
    public IReadOnlyList<MotorcycleOrder> RecentOrders { get; init; } = Array.Empty<MotorcycleOrder>();
    public IReadOnlyList<AdminTopProductStatistics> TopSellingProducts { get; init; } = Array.Empty<AdminTopProductStatistics>();
    public decimal Revenue { get; init; }
    public int TotalOrders { get; init; }
    public int PendingOrders { get; init; }
    public int ShippingOrders { get; init; }
    public int CompletedOrders { get; init; }
    public int CancelledOrders { get; init; }
    public int CustomerCount { get; init; }
    public int ProductCount { get; init; }
    public int InventoryQuantity { get; init; }
    public int ReservedInventoryQuantity { get; init; }
    public int ActivePromotions { get; init; }
    public int PromotionUses { get; init; }
    public decimal PromotionValue { get; init; }
    public int ReviewCount { get; init; }
    public int PendingReviews { get; init; }
    public int FiveStarReviews { get; init; }
    public decimal AverageRating { get; init; }
    public int StaffCount { get; init; }
    public int ActiveStaffCount { get; init; }
}

public sealed record AdminTopProductStatistics(string Name, int Quantity, decimal Revenue);
