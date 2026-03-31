namespace MedicalStockManager.Models;

public class AuditLogViewModel
{
    public IReadOnlyList<AuditLog> Logs { get; set; } = Array.Empty<AuditLog>();
}
