using MyMvcApp.Data;

namespace MyMvcApp.Models;

public sealed class AdminCustomersViewModel
{
    public required AdminStatisticsViewModel Statistics { get; init; }
    public required IReadOnlyList<Customer> Customers { get; init; }
}
