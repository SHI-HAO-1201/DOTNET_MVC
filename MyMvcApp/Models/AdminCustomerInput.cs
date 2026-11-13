using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models;

public sealed class AdminCustomerInput
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(240)]
    public string Address { get; set; } = string.Empty;
}
