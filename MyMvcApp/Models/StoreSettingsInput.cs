using System.ComponentModel.DataAnnotations;

namespace MyMvcApp.Models;

public sealed class StoreSettingsInput
{
    [Required, StringLength(120)] public string StoreName { get; set; } = "MOTOSPEED";
    [Required, EmailAddress, StringLength(320)] public string SupportEmail { get; set; } = "";
    [Required, StringLength(30)] public string Hotline { get; set; } = "";
    [StringLength(30)] public string TaxCode { get; set; } = "";
    [Required, StringLength(240)] public string Address { get; set; } = "";
    [StringLength(2000)] public string Description { get; set; } = "";
    public bool PayOnDelivery { get; set; } = true;
    public bool BankTransfer { get; set; } = true;
    public bool FreeDelivery { get; set; } = true;
    public bool NotifyNewOrder { get; set; }
    public bool NotifyLowStock { get; set; }
}
