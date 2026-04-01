using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Services;

public class InventoryService(ApplicationDbContext dbContext) : IInventoryService
{
    public InventorySessionListViewModel GetSessions()
    {
        var sessions = dbContext.InventorySessions
            .AsNoTracking()
            .Include(s => s.Lines)
            .OrderByDescending(s => s.Date)
            .ThenByDescending(s => s.Id)
            .ToList();

        return new InventorySessionListViewModel { Sessions = sessions };
    }

    public InventoryCreateViewModel GetCreateModel()
    {
        var items = dbContext.StockItems
            .AsNoTracking()
            .Include(i => i.Service)
            .OrderBy(i => i.Service != null ? i.Service.Name : "")
            .ThenBy(i => i.Name)
            .ToList();

        return new InventoryCreateViewModel
        {
            Title = $"Inventaire du {DateTime.Today:dd/MM/yyyy}",
            Lines = items.Select(i => new InventoryLineInput
            {
                StockItemId = i.Id,
                ItemName = i.Name,
                Reference = i.Reference,
                ServiceName = i.Service?.Name ?? "Inconnu",
                TheoreticalQuantity = i.CurrentQuantity,
                Unit = i.Unit,
                CountedQuantity = i.CurrentQuantity
            }).ToList()
        };
    }

    public bool CreateSession(InventoryCreateViewModel input, string username, bool applyAdjustments, out string? errorMessage)
    {
        var session = new InventorySession
        {
            Title = input.Title,
            Notes = input.Notes,
            Date = DateTime.Today,
            CreatedBy = username,
            IsCompleted = true
        };

        var lines = input.Lines.Select(l => new InventoryLine
        {
            StockItemId = l.StockItemId,
            TheoreticalQuantity = l.TheoreticalQuantity,
            CountedQuantity = l.CountedQuantity,
            Notes = l.Notes
        }).ToList();

        session.Lines = lines;
        dbContext.InventorySessions.Add(session);

        if (applyAdjustments)
        {
            foreach (var line in input.Lines)
            {
                var variance = line.CountedQuantity - line.TheoreticalQuantity;
                if (variance == 0) continue;

                var item = dbContext.StockItems.FirstOrDefault(i => i.Id == line.StockItemId);
                if (item is null) continue;

                item.CurrentQuantity = line.CountedQuantity;
                dbContext.StockMovements.Add(new StockMovement
                {
                    StockItemId = line.StockItemId,
                    MovementType = MovementType.Ajustement,
                    Quantity = Math.Abs(variance),
                    Date = DateTime.Today,
                    Notes = $"Ajustement inventaire: {input.Title} (ecart {(variance > 0 ? "+" : "")}{variance})"
                });
            }
        }

        dbContext.SaveChanges();
        errorMessage = null;
        return true;
    }

    public InventorySession? GetSessionDetails(int id)
    {
        return dbContext.InventorySessions
            .AsNoTracking()
            .Include(s => s.Lines)
            .ThenInclude(l => l.StockItem)
            .FirstOrDefault(s => s.Id == id);
    }
}
