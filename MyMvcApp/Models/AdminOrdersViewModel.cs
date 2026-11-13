using MyMvcApp.Data;

namespace MyMvcApp.Models;

public sealed class AdminOrdersViewModel
{
    public required AdminStatisticsViewModel Statistics { get; init; }
    public required IReadOnlyList<MotorcycleOrder> Orders { get; init; }
}
