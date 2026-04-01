using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class StockBatch
{
    public int Id { get; set; }

    [Required]
    public int StockItemId { get; set; }
    public StockItem? StockItem { get; set; }

    [Required]
    public int LocationId { get; set; }
    public Location? Location { get; set; }

    [Required]
    [MaxLength(80)]
    [Display(Name = "Numero de Lot")]
    public string BatchNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date d'expiration")]
    public DateTime ExpirationDate { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Quantite du Lot")]
    public int Quantity { get; set; }
}
