using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class PurchaseRequestLine
{
    public int Id { get; set; }

    public int PurchaseRequestId { get; set; }
    public PurchaseRequest? PurchaseRequest { get; set; }

    public int? StockItemId { get; set; }
    public StockItem? StockItem { get; set; }

    [Required]
    [MaxLength(150)]
    [Display(Name = "Designation")]
    public string ItemLabel { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Quantite demandee")]
    public int RequestedQuantity { get; set; }

    [MaxLength(30)]
    [Display(Name = "Unite")]
    public string Unit { get; set; } = "unite";

    [Range(0, double.MaxValue)]
    [Display(Name = "Prix unitaire estime (DA)")]
    public decimal EstimatedUnitPrice { get; set; }

    [MaxLength(200)]
    [Display(Name = "Justification de la ligne")]
    public string? Notes { get; set; }

    public decimal EstimatedTotal => RequestedQuantity * EstimatedUnitPrice;
}
