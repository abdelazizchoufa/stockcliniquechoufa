using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class SupplierFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Fournisseur")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Contact")]
    public string? ContactName { get; set; }

    [Display(Name = "Telephone")]
    public string? Phone { get; set; }

    [Display(Name = "Email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "Adresse")]
    public string? Address { get; set; }
}
