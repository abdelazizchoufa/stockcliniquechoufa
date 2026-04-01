using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalStockManager.Models;

public class StockFilterViewModel
{
    [Display(Name = "Recherche")]
    public string? SearchTerm { get; set; }

    [Display(Name = "Service")]
    public int? ServiceId { get; set; }

    public IReadOnlyList<SelectListItem> Services { get; set; } = Array.Empty<SelectListItem>();

    [Display(Name = "Stock bas seulement")]
    public bool LowStockOnly { get; set; }

    [Display(Name = "Expire bientot")]
    public bool ExpiringSoonOnly { get; set; }
}
