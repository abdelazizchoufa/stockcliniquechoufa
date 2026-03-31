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
            .Where(movement =>
                movement.MovementType == MovementType.Sortie &&
                movement.Date.Date >= effectiveStartDate &&
                movement.Date.Date <= effectiveEndDate)
            .ToList();

        var validMovements = movements
            .Where(movement => movement.StockItem is not null)
            .ToList();

        var departmentSummaries = validMovements
            .GroupBy(movement => movement.StockItem!.Department)
            .Select(group => new DepartmentConsumptionSummaryViewModel
            {
                Department = group.Key,
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
                movement.StockItem.Department
            })
            .Select(group => new ItemConsumptionSummaryViewModel
            {
                ItemName = group.Key.Name,
                Reference = group.Key.Reference,
                Department = group.Key.Department,
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
}
