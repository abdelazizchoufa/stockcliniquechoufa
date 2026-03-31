using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Services;

public class AuditService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor) : IAuditService
{
    public void Log(string action, string entityType, string? entityId, string? details, string? username = null)
    {
        var effectiveUsername = username ?? httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";

        dbContext.AuditLogs.Add(new AuditLog
        {
            Username = effectiveUsername,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            OccurredAt = DateTime.UtcNow
        });

        dbContext.SaveChanges();
    }

    public AuditLogViewModel GetRecentLogs()
    {
        var logs = dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.OccurredAt)
            .Take(100)
            .ToList();

        return new AuditLogViewModel
        {
            Logs = logs
        };
    }
}
