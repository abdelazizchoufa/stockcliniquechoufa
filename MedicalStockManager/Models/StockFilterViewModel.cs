using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class StockFilterViewModel
{
    [Display(Name = "Recherche")]
    public string? SearchTerm { get; set; }

    [Display(Name = "Service")]
    public Department? Department { get; set; }

    [Display(Name = "Stock bas seulement")]
    public bool LowStockOnly { get; set; }

    [Display(Name = "Expire bientot")]
    public bool ExpiringSoonOnly { get; set; }
}
