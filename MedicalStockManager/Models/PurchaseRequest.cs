using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public enum PurchaseRequestStatus
{
    EnAttente = 1,
    Approuvee = 2,
    Rejetee = 3,
    ConvertieEnCommande = 4
}

public class PurchaseRequest
{
    public int Id { get; set; }

    [Required]
    [MaxLength(60)]
    [Display(Name = "Numero de demande")]
    public string RequestNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    [Display(Name = "Demandeur")]
    public string RequestedBy { get; set; } = string.Empty;

    [Display(Name = "Date de demande")]
    public DateTime RequestDate { get; set; } = DateTime.Today;

    [Display(Name = "Statut")]
    public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.EnAttente;

    [MaxLength(300)]
    [Display(Name = "Justification")]
    public string? Justification { get; set; }

    [MaxLength(80)]
    [Display(Name = "Approuve / Rejete par")]
    public string? ProcessedBy { get; set; }

    [Display(Name = "Date de traitement")]
    public DateTime? ProcessedAt { get; set; }

    [MaxLength(300)]
    [Display(Name = "Motif de rejet")]
    public string? RejectionReason { get; set; }

    public int? LinkedOrderId { get; set; }
    public PurchaseOrder? LinkedOrder { get; set; }

    public ICollection<PurchaseRequestLine> Lines { get; set; } = new List<PurchaseRequestLine>();
}
