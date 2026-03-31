using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class PurchaseOrderLineInputModel
{
    [Display(Name = "Article")]
    public int? StockItemId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Quantite commandee")]
    public int? QuantityOrdered { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Prix unitaire")]
    public decimal? UnitPrice { get; set; }

    public bool HasValue =>
        StockItemId.HasValue ||
        QuantityOrdered.HasValue ||
        UnitPrice.HasValue;
}
