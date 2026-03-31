using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class StockItemFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Article")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Code interne")]
    public string Reference { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Service")]
    public Department Department { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Quantite en stock")]
    public int CurrentQuantity { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Seuil d'alerte")]
    public int AlertThreshold { get; set; }

    [Required]
    [Display(Name = "Unite")]
    public string Unit { get; set; } = "unite";

    [Display(Name = "Date d'expiration")]
    [DataType(DataType.Date)]
    public DateTime? ExpirationDate { get; set; }
}
