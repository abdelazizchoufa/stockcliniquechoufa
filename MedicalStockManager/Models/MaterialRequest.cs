using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class MaterialRequest
{
    public int Id { get; set; }

    [Required]
    [MaxLength(60)]
    [Display(Name = "Numero de Demande")]
    public string RequestNumber { get; set; } = string.Empty;

    [Required]
    public int RequestingServiceId { get; set; }
    public Service? RequestingService { get; set; }

    [Required]
    [MaxLength(80)]
    [Display(Name = "Demandeur (Utilisateur)")]
    public string RequestedByUsername { get; set; } = string.Empty;

    [Display(Name = "Date de demande")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RequestStatus Status { get; set; } = RequestStatus.Brouillon;

    [MaxLength(300)]
    [Display(Name = "Justification / Notes")]
    public string? Notes { get; set; }

    [MaxLength(80)]
    public string? ProcessedByUsername { get; set; }
    
    [MaxLength(300)]
    public string? RejectionReason { get; set; }

    public ICollection<MaterialRequestLine> Lines { get; set; } = new List<MaterialRequestLine>();
}
