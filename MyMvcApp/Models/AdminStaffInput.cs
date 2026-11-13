using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models;

public sealed class AdminStaffInput
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(320)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(80)] public string Role { get; set; } = "Nhân viên bán hàng";
    [StringLength(120)] public string StoreLocation { get; set; } = "Showroom chính";
    public bool IsActive { get; set; } = true;
}
