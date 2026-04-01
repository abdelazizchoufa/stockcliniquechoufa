using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class StockMovement
{
    public int Id { get; set; }

    [Display(Name = "Article")]
    public int StockItemId { get; set; }

    [Display(Name = "Type de mouvement")]
    public MovementType MovementType { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Quantite")]
    public int Quantity { get; set; }

    [Display(Name = "Date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Display(Name = "Observation")]
    public string? Notes { get; set; }

    [Display(Name = "Numero de lot")]
    [MaxLength(80)]
    public string? BatchNumber { get; set; }

    public StockItem? StockItem { get; set; }
}
