using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class MaterialRequestLine
{
    public int Id { get; set; }

    [Required]
    public int MaterialRequestId { get; set; }
    public MaterialRequest? MaterialRequest { get; set; }

    [Required]
    public int StockItemId { get; set; }
    public StockItem? StockItem { get; set; }

    [Range(1, 10000)]
    [Display(Name = "Quantite demandee")]
    public int RequestedQuantity { get; set; }

    [Range(0, 10000)]
    [Display(Name = "Quantite approuvee (livree)")]
    public int ApprovedQuantity { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }
}
