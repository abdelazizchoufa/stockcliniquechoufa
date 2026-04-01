using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class InventoryLine
{
    public int Id { get; set; }

    public int InventorySessionId { get; set; }
    public InventorySession? InventorySession { get; set; }

    public int StockItemId { get; set; }
    public StockItem? StockItem { get; set; }

    [Display(Name = "Stock theorique")]
    public int TheoreticalQuantity { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Stock physique compte")]
    public int CountedQuantity { get; set; }

    public int Variance => CountedQuantity - TheoreticalQuantity;

    [MaxLength(200)]
    [Display(Name = "Observation")]
    public string? Notes { get; set; }
}
