using MyMvcApp.Data;

namespace MyMvcApp.Models;

public sealed class AdminStaffViewModel
{
    public required AdminStatisticsViewModel Statistics { get; init; }
    public required IReadOnlyList<StaffMember> Members { get; init; }
}
