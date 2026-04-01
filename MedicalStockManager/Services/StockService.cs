using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Services;

public class StockService(ApplicationDbContext dbContext) : IStockService
{
    public StockIndexViewModel GetStockIndex(StockFilterViewModel filter)
    {
        var query = dbContext.StockItems.AsNoTracking().Include(i => i.Service).AsQueryable();
        var expiringLimit = DateTime.Today.AddMonths(3);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLower();
            query = query.Where(item =>
                item.Name.ToLower().Contains(search) ||
                item.Reference.ToLower().Contains(search));
        }

        if (filter.ServiceId.HasValue)
        {
            query = query.Where(item => item.ServiceId == filter.ServiceId.Value);
        }

        if (filter.LowStockOnly)
        {
            query = query.Where(item => item.CurrentQuantity <= item.AlertThreshold);
        }

        if (filter.ExpiringSoonOnly)
        {
            query = query.Where(item => item.ExpirationDate.HasValue && item.ExpirationDate <= expiringLimit);
        }

        var totalCount = query.Count();

        var items = query
            .OrderBy(item => item.Service != null ? item.Service.Name : "")
            .ThenBy(item => item.Name)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        filter.Services = GetServiceSelectList();

        return new StockIndexViewModel
        {
            Filter = filter,
            Items = items,
            TotalCount = totalCount
        };
    }

    public StockItem? GetItem(int id)
    {
        return dbContext.StockItems
            .AsNoTracking()
            .Include(i => i.Service)
            .FirstOrDefault(item => item.Id == id);
    }

    public StockItemDetailsViewModel? GetItemDetails(int id)
    {
        var item = dbContext.StockItems
            .AsNoTracking()
            .Include(i => i.Service)
            .FirstOrDefault(stockItem => stockItem.Id == id);

        if (item is null) return null;

        var batches = dbContext.StockBatches
            .AsNoTracking()
            .Include(b => b.Location)
            .Where(b => b.StockItemId == id && b.Quantity > 0)
            .OrderBy(b => b.ExpirationDate)
            .ToList();

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
            },
            Locations = GetLocationSelectList(),
            Batches = batches
        };
    }

    public StockItemFormViewModel GetCreateModel()
    {
        return new StockItemFormViewModel { Services = GetServiceSelectList() };
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
                ServiceId = item.ServiceId,
                CurrentQuantity = item.CurrentQuantity,
                AlertThreshold = item.AlertThreshold,
                Unit = item.Unit,
                ExpirationDate = item.ExpirationDate,
                Services = GetServiceSelectList()
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
            ServiceId = item.ServiceId,
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
        existingItem.ServiceId = item.ServiceId;
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
            .Include(i => i.Service)
            .FirstOrDefault(stockItem => stockItem.Id == id);

        if (item is null) return null;

        var hasMovements = dbContext.StockMovements.Any(movement => movement.StockItemId == id);
        var hasOrderLines = dbContext.PurchaseOrderLines.Any(line => line.StockItemId == id);
        var canDelete = !hasMovements && !hasOrderLines;

        return new StockItemDeleteViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Reference = item.Reference,
            ServiceName = item.Service?.Name ?? "",
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

        var locationId = input.LocationId ?? dbContext.Locations.FirstOrDefault(l => l.IsCentral)?.Id ?? 0;

        if (input.MovementType == MovementType.Entree)
        {
            var batchNum = input.BatchNumber ?? "LOT-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            var batch = dbContext.StockBatches.FirstOrDefault(b => b.StockItemId == item.Id && b.LocationId == locationId && b.BatchNumber == batchNum);
            if (batch == null)
            {
                batch = new StockBatch
                {
                    StockItemId = item.Id,
                    LocationId = locationId,
                    BatchNumber = batchNum,
                    ExpirationDate = input.ExpirationDate ?? DateTime.Today.AddYears(1),
                    Quantity = 0
                };
                dbContext.StockBatches.Add(batch);
            }

            batch.Quantity += input.Quantity;
            item.CurrentQuantity += input.Quantity;

            var movement = new StockMovement
            {
                StockItemId = input.StockItemId,
                StockBatch = batch,
                MovementType = MovementType.Entree,
                Quantity = input.Quantity,
                Date = input.Date,
                Notes = input.Notes,
                BatchNumber = batch.BatchNumber,
                DestinationLocationId = locationId
            };
            dbContext.StockMovements.Add(movement);
        }
        else if (input.MovementType == MovementType.Sortie)
        {
            var batches = dbContext.StockBatches
                .Where(b => b.StockItemId == item.Id && b.LocationId == locationId && b.Quantity > 0)
                .OrderBy(b => b.ExpirationDate)
                .ToList();

            var totalAvailable = batches.Sum(b => b.Quantity);
            if (totalAvailable < input.Quantity)
            {
                errorMessage = $"Stock insuffisant dans cet emplacement. Disponible: {totalAvailable}";
                return false;
            }

            var qtyToProcess = input.Quantity;
            foreach (var batch in batches)
            {
                if (qtyToProcess <= 0) break;

                var qtyToTake = Math.Min(batch.Quantity, qtyToProcess);
                batch.Quantity -= qtyToTake;
                qtyToProcess -= qtyToTake;
                item.CurrentQuantity -= qtyToTake;

                var movement = new StockMovement
                {
                    StockItemId = input.StockItemId,
                    StockBatch = batch,
                    MovementType = MovementType.Sortie,
                    Quantity = qtyToTake,
                    Date = input.Date,
                    Notes = input.Notes,
                    BatchNumber = batch.BatchNumber,
                    SourceLocationId = locationId
                };
                dbContext.StockMovements.Add(movement);
            }
        }
        else if (input.MovementType == MovementType.Transfert)
        {
            if (!input.DestinationLocationId.HasValue) 
            { 
                errorMessage = "Emplacement de destination requis."; 
                return false; 
            }
            
            var batches = dbContext.StockBatches
                .Where(b => b.StockItemId == item.Id && b.LocationId == locationId && b.Quantity > 0)
                .OrderBy(b => b.ExpirationDate)
                .ToList();

            var totalAvailable = batches.Sum(b => b.Quantity);
            if (totalAvailable < input.Quantity)
            {
                errorMessage = $"Stock insuffisant pour le transfert. Disponible: {totalAvailable}";
                return false;
            }

            var qtyToProcess = input.Quantity;
            foreach (var batch in batches)
            {
                if (qtyToProcess <= 0) break;

                var qtyToTake = Math.Min(batch.Quantity, qtyToProcess);
                batch.Quantity -= qtyToTake;
                qtyToProcess -= qtyToTake;
                
                var destBatch = dbContext.StockBatches.FirstOrDefault(b => b.StockItemId == item.Id && b.LocationId == input.DestinationLocationId.Value && b.BatchNumber == batch.BatchNumber);
                if (destBatch == null)
                {
                    destBatch = new StockBatch
                    {
                        StockItemId = item.Id,
                        LocationId = input.DestinationLocationId.Value,
                        BatchNumber = batch.BatchNumber,
                        ExpirationDate = batch.ExpirationDate,
                        Quantity = 0
                    };
                    dbContext.StockBatches.Add(destBatch);
                }
                destBatch.Quantity += qtyToTake;

                var movement = new StockMovement
                {
                    StockItemId = input.StockItemId,
                    StockBatch = batch,
                    MovementType = MovementType.Transfert,
                    Quantity = qtyToTake,
                    Date = input.Date,
                    Notes = input.Notes,
                    BatchNumber = batch.BatchNumber,
                    SourceLocationId = locationId,
                    DestinationLocationId = input.DestinationLocationId.Value
                };
                dbContext.StockMovements.Add(movement);
            }
        }
        else 
        {
            errorMessage = "Ajustement non supporte via cette nouvelle interface de Lots FEFO.";
            return false;
        }

        dbContext.SaveChanges();
        errorMessage = null;
        return true;
    }

    public DashboardViewModel GetDashboard()
    {
        var items = dbContext.StockItems
            .AsNoTracking()
            .Include(i => i.Service)
            .ToList();
        var recentMovements = dbContext.StockMovements
            .AsNoTracking()
            .Include(movement => movement.StockItem)
            .ThenInclude(si => si!.Service)
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
                    ServiceName = item.Service?.Name ?? "",
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
                    ServiceName = item.Service?.Name ?? "",
                    Message = $"Expiration le {item.ExpirationDate:dd/MM/yyyy}",
                    Severity = "danger"
                })
                .ToList(),
            DepartmentSummaries = items
                .GroupBy(item => item.Service?.Name ?? "Inconnu")
                .Select(group => new DepartmentDashboardSummaryViewModel
                {
                    ServiceName = group.Key,
                    ItemCount = group.Count(),
                    LowStockCount = group.Count(item => item.IsLowStock),
                    TotalQuantity = group.Sum(item => item.CurrentQuantity)
                })
                .OrderBy(summary => summary.ServiceName)
                .ToList(),
            RecentMovements = recentMovements
                .Where(movement => movement.StockItem is not null)
                .Select(movement => new RecentMovementViewModel
                {
                    StockItemId = movement.StockItemId,
                    ItemName = movement.StockItem!.Name,
                    Reference = movement.StockItem.Reference,
                    ServiceName = movement.StockItem.Service?.Name ?? "",
                    MovementType = movement.MovementType,
                    Quantity = movement.Quantity,
                    Date = movement.Date,
                    Notes = movement.Notes
                })
                .ToList()
        };
    }

    public StockItemDetailsViewModel? GetItemDetailsFiltered(int id, MovementFilterViewModel filter)
    {
        var item = dbContext.StockItems.AsNoTracking().Include(i => i.Service).FirstOrDefault(i => i.Id == id);
        if (item is null) return null;

        var batches = dbContext.StockBatches
            .AsNoTracking()
            .Include(b => b.Location)
            .Where(b => b.StockItemId == id && b.Quantity > 0)
            .OrderBy(b => b.ExpirationDate)
            .ToList();

        var query = dbContext.StockMovements.AsNoTracking().Where(m => m.StockItemId == id);

        if (filter.DateFrom.HasValue)
            query = query.Where(m => m.Date >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue)
            query = query.Where(m => m.Date <= filter.DateTo.Value.AddDays(1));
        if (filter.MovementType.HasValue)
            query = query.Where(m => m.MovementType == filter.MovementType.Value);

        var movements = query
            .OrderByDescending(m => m.Date)
            .ThenByDescending(m => m.Id)
            .ToList();

        return new StockItemDetailsViewModel
        {
            Item = item,
            Movements = movements,
            NewMovement = new StockMovementInputModel { StockItemId = id, Date = DateTime.Today },
            Filter = filter,
            Locations = GetLocationSelectList(),
            Batches = batches
        };
    }

    public ExpirationReportViewModel GetExpirationReport()
    {
        var today = DateTime.Today;
        var items = dbContext.StockItems.AsNoTracking()
            .Include(i => i.Service)
            .Where(i => i.ExpirationDate.HasValue)
            .OrderBy(i => i.ExpirationDate)
            .ToList();

        return new ExpirationReportViewModel
        {
            ExpiredItems = items
                .Where(i => i.ExpirationDate!.Value < today)
                .Select(i => ToExpirationItem(i, today))
                .ToList(),
            ExpiringIn7Days = items
                .Where(i => i.ExpirationDate!.Value >= today && i.ExpirationDate.Value <= today.AddDays(7))
                .Select(i => ToExpirationItem(i, today))
                .ToList(),
            ExpiringIn30Days = items
                .Where(i => i.ExpirationDate!.Value > today.AddDays(7) && i.ExpirationDate.Value <= today.AddDays(30))
                .Select(i => ToExpirationItem(i, today))
                .ToList(),
            ExpiringIn90Days = items
                .Where(i => i.ExpirationDate!.Value > today.AddDays(30) && i.ExpirationDate.Value <= today.AddDays(90))
                .Select(i => ToExpirationItem(i, today))
                .ToList()
        };
    }

    public ServiceDirectoryViewModel GetServiceDirectory()
    {
        var services = dbContext.Services
            .AsNoTracking()
            .Select(s => new ServiceListItemViewModel
            {
                Id = s.Id,
                Name = s.Name,
                ItemCount = dbContext.StockItems.Count(i => i.ServiceId == s.Id)
            })
            .OrderBy(s => s.Name)
            .ToList();

        return new ServiceDirectoryViewModel
        {
            Services = services
        };
    }

    public bool AddService(ServiceCreateInputViewModel input, out string? errorMessage)
    {
        var normalizedName = input.Name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            errorMessage = "Le nom du service est obligatoire.";
            return false;
        }

        var exists = dbContext.Services.Any(s => s.Name.ToLower() == normalizedName.ToLower());
        if (exists)
        {
            errorMessage = "Ce service existe deja.";
            return false;
        }

        dbContext.Services.Add(new Service { Name = normalizedName });
        dbContext.SaveChanges();
        errorMessage = null;
        return true;
    }

    public bool DeleteService(int id, out string? errorMessage)
    {
        var service = dbContext.Services.FirstOrDefault(s => s.Id == id);
        if (service is null)
        {
            errorMessage = "Service introuvable.";
            return false;
        }

        var hasItems = dbContext.StockItems.Any(i => i.ServiceId == id);
        if (hasItems)
        {
            errorMessage = "Ce service est utilise par des articles et ne peut pas etre supprime.";
            return false;
        }

        dbContext.Services.Remove(service);
        dbContext.SaveChanges();
        errorMessage = null;
        return true;
    }

    private static ExpirationItemViewModel ToExpirationItem(StockItem i, DateTime today)
    {
        var days = (i.ExpirationDate!.Value - today).Days;
        return new ExpirationItemViewModel
        {
            Id = i.Id,
            Name = i.Name,
            Reference = i.Reference,
            ServiceName = i.Service?.Name ?? "",
            CurrentQuantity = i.CurrentQuantity,
            Unit = i.Unit,
            ExpirationDate = i.ExpirationDate.Value,
            DaysRemaining = days,
            Severity = days < 0 ? "danger" : days <= 7 ? "danger" : days <= 30 ? "warning" : "info"
        };
    }

    private IReadOnlyList<SelectListItem> GetServiceSelectList()
    {
        return dbContext.Services
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            .ToList();
    }
    private IReadOnlyList<SelectListItem> GetLocationSelectList()
    {
        return dbContext.Locations
            .AsNoTracking()
            .OrderByDescending(l => l.IsCentral)
            .ThenBy(l => l.Name)
            .Select(l => new SelectListItem { Value = l.Id.ToString(), Text = l.Name })
            .ToList();
    }
}
