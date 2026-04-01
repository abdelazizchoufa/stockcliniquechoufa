using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Services;

public class ReportService(ApplicationDbContext dbContext) : IReportService
{
    public ConsumptionReportViewModel GetConsumptionReport(DateTime? startDate, DateTime? endDate)
    {
        var effectiveEndDate = (endDate ?? DateTime.Today).Date;
        var effectiveStartDate = (startDate ?? effectiveEndDate.AddDays(-30)).Date;

        if (effectiveStartDate > effectiveEndDate)
        {
            (effectiveStartDate, effectiveEndDate) = (effectiveEndDate, effectiveStartDate);
        }

        var movements = dbContext.StockMovements
            .AsNoTracking()
            .Include(movement => movement.StockItem)
            .ThenInclude(stockItem => stockItem!.Service)
            .Where(movement =>
                movement.MovementType == MovementType.Sortie &&
                movement.Date.Date >= effectiveStartDate &&
                movement.Date.Date <= effectiveEndDate)
            .ToList();

        var validMovements = movements
            .Where(movement => movement.StockItem is not null)
            .ToList();

        var departmentSummaries = validMovements
            .GroupBy(movement => movement.StockItem!.Service?.Name ?? "Inconnu")
            .Select(group => new DepartmentConsumptionSummaryViewModel
            {
                ServiceName = group.Key,
                TotalQuantityOut = group.Sum(movement => movement.Quantity),
                DistinctItems = group.Select(movement => movement.StockItemId).Distinct().Count()
            })
            .OrderByDescending(summary => summary.TotalQuantityOut)
            .ToList();

        var topItems = validMovements
            .GroupBy(movement => new
            {
                movement.StockItemId,
                movement.StockItem!.Name,
                movement.StockItem.Reference,
                ServiceName = movement.StockItem.Service != null ? movement.StockItem.Service.Name : "Inconnu"
            })
            .Select(group => new ItemConsumptionSummaryViewModel
            {
                ItemName = group.Key.Name,
                Reference = group.Key.Reference,
                ServiceName = group.Key.ServiceName,
                TotalQuantityOut = group.Sum(movement => movement.Quantity),
                MovementCount = group.Count()
            })
            .OrderByDescending(item => item.TotalQuantityOut)
            .ThenBy(item => item.ItemName)
            .ToList();

        return new ConsumptionReportViewModel
        {
            Filter = new ConsumptionReportFilterViewModel
            {
                StartDate = effectiveStartDate,
                EndDate = effectiveEndDate
            },
            TotalQuantityOut = validMovements.Sum(movement => movement.Quantity),
            TotalMovements = validMovements.Count,
            DepartmentSummaries = departmentSummaries,
            TopItems = topItems
        };
    }

    public AnalyticsViewModel GetAnalytics()
    {
        var today = DateTime.Today;
        var allMovements = dbContext.StockMovements
            .AsNoTracking()
            .Include(m => m.StockItem)
            .ThenInclude(stockItem => stockItem!.Service)
            .ToList();

        var last6Months = Enumerable.Range(0, 6)
            .Select(i => today.AddMonths(-i))
            .OrderBy(d => d)
            .Select(d => new MonthlyConsumptionViewModel
            {
                Month = d.ToString("MMM yyyy"),
                TotalOut = allMovements
                    .Where(m => m.MovementType == MovementType.Sortie && m.Date.Year == d.Year && m.Date.Month == d.Month)
                    .Sum(m => m.Quantity),
                TotalIn = allMovements
                    .Where(m => m.MovementType == MovementType.Entree && m.Date.Year == d.Year && m.Date.Month == d.Month)
                    .Sum(m => m.Quantity)
            })
            .ToList();

        var topConsumers = allMovements
            .Where(m => m.MovementType == MovementType.Sortie && m.StockItem is not null)
            .GroupBy(m => new
            {
                m.StockItemId,
                m.StockItem!.Name,
                m.StockItem.Reference,
                ServiceName = m.StockItem.Service != null ? m.StockItem.Service.Name : "Inconnu"
            })
            .Select(g => new TopConsumerViewModel
            {
                ItemName = g.Key.Name,
                Reference = g.Key.Reference,
                ServiceName = g.Key.ServiceName,
                TotalConsumed = g.Sum(m => m.Quantity)
            })
            .OrderByDescending(x => x.TotalConsumed)
            .Take(10)
            .ToList();

        var movedItemIds = allMovements.Select(m => m.StockItemId).Distinct().ToHashSet();
        var neverMoved = dbContext.StockItems.AsNoTracking()
            .Include(i => i.Service)
            .Where(i => !movedItemIds.Contains(i.Id))
            .Select(i => new NeverMovedItemViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Reference = i.Reference,
                ServiceName = i.Service != null ? i.Service.Name : "Inconnu",
                CurrentQuantity = i.CurrentQuantity,
                Unit = i.Unit
            })
            .ToList();

        var thisMonthOut = allMovements
            .Where(m => m.MovementType == MovementType.Sortie && m.Date.Year == today.Year && m.Date.Month == today.Month)
            .Sum(m => m.Quantity);

        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

        return new AnalyticsViewModel
        {
            Last6Months = last6Months,
            TopConsumers = topConsumers,
            NeverMovedItems = neverMoved,
            TotalMovementsThisMonth = allMovements.Count(m => m.Date.Year == today.Year && m.Date.Month == today.Month),
            AverageDailyConsumption = daysInMonth == 0 ? 0 : Math.Round((decimal)thisMonthOut / daysInMonth, 1)
        };
    }
}
