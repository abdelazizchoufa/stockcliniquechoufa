using MedicalStockManager.Models;

namespace MedicalStockManager.Services;

public interface IAuditService
{
    void Log(string action, string entityType, string? entityId, string? details, string? username = null);
    AuditLogViewModel GetRecentLogs();
}
