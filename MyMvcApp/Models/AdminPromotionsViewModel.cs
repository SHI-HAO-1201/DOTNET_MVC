using MyMvcApp.Data;

namespace MyMvcApp.Models;

public sealed class AdminPromotionsViewModel
{
    public required AdminStatisticsViewModel Statistics { get; init; }
    public required IReadOnlyList<Promotion> Promotions { get; init; }
}
