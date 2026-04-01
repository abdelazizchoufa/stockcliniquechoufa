using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class InventorySession
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Intitule")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Date")]
    public DateTime Date { get; set; } = DateTime.Today;

    [MaxLength(300)]
    [Display(Name = "Observation")]
    public string? Notes { get; set; }

    public bool IsCompleted { get; set; }

    [MaxLength(80)]
    public string CreatedBy { get; set; } = string.Empty;

    public ICollection<InventoryLine> Lines { get; set; } = new List<InventoryLine>();
}
