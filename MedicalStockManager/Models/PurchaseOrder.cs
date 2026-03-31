using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class PurchaseOrder
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Numero commande")]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Fournisseur")]
    public int SupplierId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date commande")]
    public DateTime OrderDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Date reception prevue")]
    public DateTime? ExpectedDeliveryDate { get; set; }

    [Display(Name = "Statut")]
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Brouillon;

    [Display(Name = "Observation")]
    public string? Notes { get; set; }

    public Supplier? Supplier { get; set; }
    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}
