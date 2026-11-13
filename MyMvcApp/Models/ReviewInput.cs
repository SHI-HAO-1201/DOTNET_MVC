using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models;

public sealed class ReviewInput
{
    public int MotorcycleId { get; set; }
    [Required, StringLength(120)] public string ReviewerName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(320)] public string ReviewerEmail { get; set; } = string.Empty;
    [Range(1, 5)] public int Rating { get; set; } = 5;
    [Required, StringLength(2000, MinimumLength = 10)] public string Comment { get; set; } = string.Empty;
}
