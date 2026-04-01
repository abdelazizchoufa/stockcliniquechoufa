using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

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
    [Range(1, int.MaxValue, ErrorMessage = "Veuillez selectionner un service.")]
    [Display(Name = "Service")]
    public int ServiceId { get; set; }

    public IReadOnlyList<SelectListItem> Services { get; set; } = Array.Empty<SelectListItem>();

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
