using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Services;

public class AutoReplenishmentService(IServiceScopeFactory scopeFactory, ILogger<AutoReplenishmentService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("AutoReplenishmentService demarre.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoWorkAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de l'execution du service d'automatisation.");
            }

            // Verification toutes les 12 heures dans la vraie vie, mais pour la demo on met 30 minutes.
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Demarrage du cycle de verification automatique des stocks...");

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

        await CheckLowStockAndGenerateOrdersAsync(dbContext, auditService, cancellationToken);
        await CheckExpiringBatchesAsync(dbContext, auditService, cancellationToken);
        
        logger.LogInformation("Cycle de verification termine.");
    }

    private async Task CheckLowStockAndGenerateOrdersAsync(ApplicationDbContext dbContext, IAuditService auditService, CancellationToken cancellationToken)
    {
        // Articles sous le seuil d'alerte
        var lowStockItems = await dbContext.StockItems
            .Where(i => i.CurrentQuantity <= i.AlertThreshold)
            .ToListAsync(cancellationToken);

        if (!lowStockItems.Any()) return;

        // Trouver les brouillons de commande existants pour ne pas doubler les lignes
        var pendingOrderLinesList = await dbContext.PurchaseOrderLines
            .Include(l => l.PurchaseOrder)
            .Where(l => l.PurchaseOrder!.Status == PurchaseOrderStatus.Brouillon || l.PurchaseOrder.Status == PurchaseOrderStatus.Commandee)
            .Select(l => l.StockItemId)
            .ToListAsync(cancellationToken);
            
        var pendingOrderLines = pendingOrderLinesList.ToHashSet();

        var itemsToOrder = lowStockItems.Where(i => !pendingOrderLines.Contains(i.Id)).ToList();

        if (!itemsToOrder.Any()) return;

        // Create a draft PurchaseOrder for generic internal supply (SupplierId = 1 as default/system if exists, or null if allowed)
        var defaultSupplier = await dbContext.Suppliers.FirstOrDefaultAsync(cancellationToken);
        if (defaultSupplier == null) return; // Cannot generate PO without at least 1 supplier

        var po = new PurchaseOrder
        {
            OrderNumber = "AUTO-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            SupplierId = defaultSupplier.Id,
            OrderDate = DateTime.UtcNow,
            Status = PurchaseOrderStatus.Brouillon,
            Notes = "Généré automatiquement par le système suite à la détection de stocks bas."
        };

        foreach (var item in itemsToOrder)
        {
            var deficit = item.AlertThreshold - item.CurrentQuantity;
            var suggestedQty = deficit > 0 ? deficit + (int)(item.AlertThreshold * 0.5) : item.AlertThreshold; // commander de quoi remplir le seuil + 50%
            
            po.Lines.Add(new PurchaseOrderLine
            {
                StockItemId = item.Id,
                QuantityOrdered = suggestedQty,
                UnitPrice = 0 // A remplir par l'utilisateur
            });
        }

        dbContext.PurchaseOrders.Add(po);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        auditService.Log("Automatique", "PurchaseOrder", po.OrderNumber, $"Creation auto de brouillon pour {itemsToOrder.Count} articles en deficit");
        logger.LogWarning($"Alerte de stock bas. Creation automatique du bon de commande {po.OrderNumber}");
    }

    private async Task CheckExpiringBatchesAsync(ApplicationDbContext dbContext, IAuditService auditService, CancellationToken cancellationToken)
    {
        var limitDate = DateTime.Today.AddDays(30);
        
        var expiringBatches = await dbContext.StockBatches
            .Include(b => b.StockItem)
            .Include(b => b.Location)
            .Where(b => b.Quantity > 0 && b.ExpirationDate <= limitDate)
            .ToListAsync(cancellationToken);

        foreach (var batch in expiringBatches)
        {
            logger.LogWarning($"ALERTE PEREMPTION : Le Lot {batch.BatchNumber} ({batch.StockItem!.Name}) au magasin {batch.Location!.Name} expire le {batch.ExpirationDate:dd/MM/yyyy}");
            // Pour eviter de spammer l'audit, on pourrait verifier si on a deja logge.
            // Pour la simplicite du POC, on log.
            auditService.Log("Automatique", "Alerte Peremption", batch.BatchNumber, $"Lot expirant le {batch.ExpirationDate:dd/MM/yyyy} ({batch.Quantity} restants)");
        }
    }
}
