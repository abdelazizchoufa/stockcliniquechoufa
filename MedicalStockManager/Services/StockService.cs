using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Services;

public class StockService(ApplicationDbContext dbContext) : IStockService
{
    public StockIndexViewModel GetStockIndex(StockFilterViewModel filter)
    {
        var query = dbContext.StockItems.AsNoTracking().AsQueryable();
        var expiringLimit = DateTime.Today.AddMonths(3);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLower();
            query = query.Where(item =>
                item.Name.ToLower().Contains(search) ||
                item.Reference.ToLower().Contains(search));
        }

        if (filter.Department.HasValue)
        {
            query = query.Where(item => item.Department == filter.Department.Value);
        }

        if (filter.LowStockOnly)
        {
            query = query.Where(item => item.CurrentQuantity <= item.AlertThreshold);
        }

        if (filter.ExpiringSoonOnly)
        {
            query = query.Where(item => item.ExpirationDate.HasValue && item.ExpirationDate <= expiringLimit);
        }

        var items = query
            .OrderBy(item => item.Department)
            .ThenBy(item => item.Name)
            .ToList();

        return new StockIndexViewModel
        {
            Filter = filter,
            Items = items
        };
    }

    public StockItem? GetItem(int id)
    {
        return dbContext.StockItems
            .AsNoTracking()
            .FirstOrDefault(item => item.Id == id);
    }

    public StockItemDetailsViewModel? GetItemDetails(int id)
    {
        var item = dbContext.StockItems
            .AsNoTracking()
            .FirstOrDefault(stockItem => stockItem.Id == id);

        if (item is null)
        {
            return null;
        }

        var movements = dbContext.StockMovements
            .AsNoTracking()
            .Where(movement => movement.StockItemId == id)
            .OrderByDescending(movement => movement.Date)
            .ThenByDescending(movement => movement.Id)
            .ToList();

        return new StockItemDetailsViewModel
        {
            Item = item,
            Movements = movements,
            NewMovement = new StockMovementInputModel
            {
                StockItemId = id,
                Date = DateTime.Today
            }
        };
    }

    public StockItemFormViewModel GetCreateModel()
    {
        return new StockItemFormViewModel();
    }

    public StockItemFormViewModel? GetEditModel(int id)
    {
        return dbContext.StockItems
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new StockItemFormViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Reference = item.Reference,
                Department = item.Department,
                CurrentQuantity = item.CurrentQuantity,
                AlertThreshold = item.AlertThreshold,
                Unit = item.Unit,
                ExpirationDate = item.ExpirationDate
            })
            .FirstOrDefault();
    }

    public bool AddItem(StockItemFormViewModel item, out string? errorMessage)
    {
        if (dbContext.StockItems.Any(existing => existing.Reference == item.Reference))
        {
            errorMessage = "Cette reference existe deja.";
            return false;
        }

        dbContext.StockItems.Add(new StockItem
        {
            Name = item.Name,
            Reference = item.Reference,
            Department = item.Department,
            CurrentQuantity = item.CurrentQuantity,
            AlertThreshold = item.AlertThreshold,
            Unit = item.Unit,
            ExpirationDate = item.ExpirationDate
        });
        dbContext.SaveChanges();
        errorMessage = null;
        return true;
    }

    public bool UpdateItem(StockItemFormViewModel item, out string? errorMessage)
    {
        var existingItem = dbContext.StockItems.FirstOrDefault(stockItem => stockItem.Id == item.Id);

        if (existingItem is null)
        {
            errorMessage = "Article introuvable.";
            return false;
        }

        if (dbContext.StockItems.Any(stockItem => stockItem.Reference == item.Reference && stockItem.Id != item.Id))
        {
            errorMessage = "Cette reference est deja utilisee par un autre article.";
            return false;
        }

        existingItem.Name = item.Name;
        existingItem.Reference = item.Reference;
        existingItem.Department = item.Department;
        existingItem.CurrentQuantity = item.CurrentQuantity;
        existingItem.AlertThreshold = item.AlertThreshold;
        existingItem.Unit = item.Unit;
        existingItem.ExpirationDate = item.ExpirationDate;

        dbContext.SaveChanges();
        errorMessage = null;
        return true;
    }

    public StockItemDeleteViewModel? GetDeleteModel(int id)
    {
        var item = dbContext.StockItems
            .AsNoTracking()
            .FirstOrDefault(stockItem => stockItem.Id == id);

        if (item is null)
        {
            return null;
        }

        var hasMovements = dbContext.StockMovements.Any(movement => movement.StockItemId == id);
        var hasOrderLines = dbContext.PurchaseOrderLines.Any(line => line.StockItemId == id);
        var canDelete = !hasMovements && !hasOrderLines;

        return new StockItemDeleteViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Reference = item.Reference,
            Department = item.Department,
            CurrentQuantity = item.CurrentQuantity,
            CanDelete = canDelete,
            BlockingReason = canDelete
                ? null
                : "Cet article est lie a des mouvements ou a des commandes fournisseurs et ne peut pas etre supprime."
        };
    }

    public bool DeleteItem(int id, out string? errorMessage)
    {
        var item = dbContext.StockItems.FirstOrDefault(stockItem => stockItem.Id == id);

        if (item is null)
        {
            errorMessage = "Article introuvable.";
            return false;
        }

        if (dbContext.StockMovements.Any(movement => movement.StockItemId == id) ||
            dbContext.PurchaseOrderLines.Any(line => line.StockItemId == id))
        {
            errorMessage = "Cet article ne peut pas etre supprime car il est deja utilise dans l'historique.";
            return false;
        }

        dbContext.StockItems.Remove(item);
        dbContext.SaveChanges();
        errorMessage = null;
        return true;
    }

    public bool AddMovement(StockMovementInputModel input, out string? errorMessage)
    {
        var item = dbContext.StockItems.FirstOrDefault(stockItem => stockItem.Id == input.StockItemId);

        if (item is null)
        {
            errorMessage = "Article introuvable.";
            return false;
        }

        var signedQuantity = input.MovementType switch
        {
            MovementType.Entree => input.Quantity,
            MovementType.Sortie => -input.Quantity,
            MovementType.Ajustement => input.Quantity,
            _ => 0
        };

        if (input.MovementType == MovementType.Sortie && item.CurrentQuantity < input.Quantity)
        {
            errorMessage = "La quantite demandee depasse le stock disponible.";
            return false;
        }

        item.CurrentQuantity += signedQuantity;

        if (item.CurrentQuantity < 0)
        {
            errorMessage = "Le stock ne peut pas devenir negatif.";
            return false;
        }

        var movement = new StockMovement
        {
            StockItemId = input.StockItemId,
            MovementType = input.MovementType,
            Quantity = input.Quantity,
            Date = input.Date,
            Notes = input.Notes
        };

        dbContext.StockMovements.Add(movement);
        dbContext.SaveChanges();

        errorMessage = null;
        return true;
    }

    public DashboardViewModel GetDashboard()
    {
        var items = dbContext.StockItems
            .AsNoTracking()
            .ToList();
        var recentMovements = dbContext.StockMovements
            .AsNoTracking()
            .Include(movement => movement.StockItem)
            .OrderByDescending(movement => movement.Date)
            .ThenByDescending(movement => movement.Id)
            .Take(8)
            .ToList();

        var expiringLimit = DateTime.Today.AddMonths(3);

        return new DashboardViewModel
        {
            TotalItems = items.Count,
            LowStockItems = items.Count(item => item.IsLowStock),
            ExpiringSoonItems = items.Count(item => item.ExpirationDate is not null && item.ExpirationDate <= expiringLimit),
            ItemsByPriority = items
                .OrderByDescending(item => item.IsLowStock)
                .ThenBy(item => item.ExpirationDate ?? DateTime.MaxValue)
                .ThenBy(item => item.Name)
                .Take(10)
                .ToList(),
            LowStockAlerts = items
                .Where(item => item.IsLowStock)
                .OrderBy(item => item.CurrentQuantity)
                .ThenBy(item => item.Name)
                .Take(5)
                .Select(item => new AlertItemViewModel
                {
                    StockItemId = item.Id,
                    ItemName = item.Name,
                    Reference = item.Reference,
                    Department = item.Department,
                    Message = $"Stock actuel {item.CurrentQuantity} {item.Unit} pour un seuil de {item.AlertThreshold}",
                    Severity = "warning"
                })
                .ToList(),
            ExpirationAlerts = items
                .Where(item => item.ExpirationDate is not null && item.ExpirationDate <= expiringLimit)
                .OrderBy(item => item.ExpirationDate)
                .ThenBy(item => item.Name)
                .Take(5)
                .Select(item => new AlertItemViewModel
                {
                    StockItemId = item.Id,
                    ItemName = item.Name,
                    Reference = item.Reference,
                    Department = item.Department,
                    Message = $"Expiration le {item.ExpirationDate:dd/MM/yyyy}",
                    Severity = "danger"
                })
                .ToList(),
            DepartmentSummaries = items
                .GroupBy(item => item.Department)
                .Select(group => new DepartmentDashboardSummaryViewModel
                {
                    Department = group.Key,
                    ItemCount = group.Count(),
                    LowStockCount = group.Count(item => item.IsLowStock),
                    TotalQuantity = group.Sum(item => item.CurrentQuantity)
                })
                .OrderBy(summary => summary.Department)
                .ToList(),
            RecentMovements = recentMovements
                .Where(movement => movement.StockItem is not null)
                .Select(movement => new RecentMovementViewModel
                {
                    StockItemId = movement.StockItemId,
                    ItemName = movement.StockItem!.Name,
                    Reference = movement.StockItem.Reference,
                    Department = movement.StockItem.Department,
                    MovementType = movement.MovementType,
                    Quantity = movement.Quantity,
                    Date = movement.Date,
                    Notes = movement.Notes
                })
                .ToList()
        };
    }
}
