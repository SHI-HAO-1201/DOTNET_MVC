using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models;

public sealed class ProductContactRequestInput
{
    public int MotorcycleId { get; set; }

    [Required, StringLength(120, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress, StringLength(320)]
    public string? Email { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }
}
