using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalStockManager.Models;

public class PurchaseOrderCreateViewModel
{
    [Required]
    [Display(Name = "Numero commande")]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Fournisseur")]
    public int SupplierId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date commande")]
    public DateTime OrderDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Date reception prevue")]
    public DateTime? ExpectedDeliveryDate { get; set; }

    [Display(Name = "Observation")]
    public string? Notes { get; set; }

    public IReadOnlyList<SelectListItem> Suppliers { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> StockItems { get; set; } = Array.Empty<SelectListItem>();
    public List<PurchaseOrderLineInputModel> Lines { get; set; } = [];
}
