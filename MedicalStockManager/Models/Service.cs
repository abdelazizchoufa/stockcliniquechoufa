using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class Service
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Service")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}
