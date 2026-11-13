using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models;

public sealed class AccountProfileViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên cần từ 2 đến 100 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(240, ErrorMessage = "Địa chỉ tối đa 240 ký tự.")]
    public string? Address { get; set; }
}
