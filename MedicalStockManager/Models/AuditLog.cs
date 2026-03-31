using System.ComponentModel.DataAnnotations;

namespace MedicalStockManager.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string EntityType { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? EntityId { get; set; }

    [MaxLength(300)]
    public string? Details { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
