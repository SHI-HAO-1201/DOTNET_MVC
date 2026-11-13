using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Data;

public sealed class ProductContactRequest
{
    public int Id { get; set; }
    public int MotorcycleId { get; set; }
    public Motorcycle Motorcycle { get; set; } = null!;

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress, StringLength(320)]
    public string? Email { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsContacted { get; set; }
}
