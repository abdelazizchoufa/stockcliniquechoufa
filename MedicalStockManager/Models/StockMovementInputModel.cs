using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class StockMovementInputModel
{
    [Required]
    public int StockItemId { get; set; }

    [Required]
    [Display(Name = "Type de mouvement")]
    public MovementType MovementType { get; set; } = MovementType.Entree;

    [Range(1, int.MaxValue)]
    [Display(Name = "Quantite")]
    public int Quantity { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date")]
    public DateTime Date { get; set; } = DateTime.Today;

    [Display(Name = "Observation")]
    [StringLength(300)]
    public string? Notes { get; set; }

    [Display(Name = "Numero de lot")]
    [StringLength(80)]
    public string? BatchNumber { get; set; }
}
