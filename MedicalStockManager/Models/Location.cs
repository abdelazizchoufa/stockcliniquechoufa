using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class Location
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Nom de l'emplacement")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Est le magasin central ?")]
    public bool IsCentral { get; set; }

    public ICollection<StockBatch> Batches { get; set; } = new List<StockBatch>();
}
