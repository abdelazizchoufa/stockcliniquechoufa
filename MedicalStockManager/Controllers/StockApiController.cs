using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockApiController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet("items")]
    public IActionResult GetItems([FromQuery] string? search, [FromQuery] int? serviceId, [FromQuery] bool lowStockOnly = false)
    {
        var query = dbContext.StockItems.AsNoTracking().Include(i => i.Service).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(s) || i.Reference.ToLower().Contains(s));
        }
        if (serviceId.HasValue)
            query = query.Where(i => i.ServiceId == serviceId.Value);
        if (lowStockOnly)
            query = query.Where(i => i.CurrentQuantity <= i.AlertThreshold);

        var items = query.OrderBy(i => i.Name).Select(i => new
        {
            i.Id,
            i.Name,
            i.Reference,
            ServiceName = i.Service != null ? i.Service.Name : string.Empty,
            i.CurrentQuantity,
            i.AlertThreshold,
            i.Unit,
            ExpirationDate = i.ExpirationDate.HasValue ? i.ExpirationDate.Value.ToString("yyyy-MM-dd") : null,
            IsLowStock = i.CurrentQuantity <= i.AlertThreshold
        }).ToList();

        return Ok(items);
    }

    [HttpGet("items/{id}")]
    public IActionResult GetItem(int id)
    {
        var item = dbContext.StockItems.AsNoTracking().Include(i => i.Service).FirstOrDefault(i => i.Id == id);
        if (item is null) return NotFound();

        var movements = dbContext.StockMovements.AsNoTracking()
            .Where(m => m.StockItemId == id)
            .OrderByDescending(m => m.Date)
            .Take(20)
            .Select(m => new
            {
                m.Id,
                Type = m.MovementType.ToString(),
                m.Quantity,
                Date = m.Date.ToString("yyyy-MM-dd"),
                m.Notes,
                m.BatchNumber
            }).ToList();

        return Ok(new
        {
            item.Id, item.Name, item.Reference,
            ServiceName = item.Service?.Name ?? string.Empty,
            item.CurrentQuantity, item.AlertThreshold, item.Unit,
            ExpirationDate = item.ExpirationDate?.ToString("yyyy-MM-dd"),
            IsLowStock = item.CurrentQuantity <= item.AlertThreshold,
            RecentMovements = movements
        });
    }

    [HttpGet("alerts")]
    public IActionResult GetAlerts()
    {
        var today = DateTime.Today;
        var expiringLimit = today.AddDays(30);

        var lowStock = dbContext.StockItems.AsNoTracking()
            .Where(i => i.CurrentQuantity <= i.AlertThreshold)
            .Select(i => new { i.Id, i.Name, i.Reference, Type = "LowStock", Message = $"Stock {i.CurrentQuantity}/{i.AlertThreshold}" })
            .ToList();

        var expiring = dbContext.StockItems.AsNoTracking()
            .Where(i => i.ExpirationDate.HasValue && i.ExpirationDate.Value <= expiringLimit)
            .Select(i => new { i.Id, i.Name, i.Reference, Type = "Expiration", Message = $"Expire le {i.ExpirationDate!.Value:dd/MM/yyyy}" })
            .ToList();

        return Ok(new { LowStock = lowStock, Expiring = expiring, TotalAlerts = lowStock.Count + expiring.Count });
    }

    [HttpGet("movements")]
    public IActionResult GetMovements([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? itemId)
    {
        var query = dbContext.StockMovements.AsNoTracking().AsQueryable();

        if (from.HasValue) query = query.Where(m => m.Date >= from.Value);
        if (to.HasValue) query = query.Where(m => m.Date <= to.Value.AddDays(1));
        if (itemId.HasValue) query = query.Where(m => m.StockItemId == itemId.Value);

        var movements = query.OrderByDescending(m => m.Date).Take(100)
            .Select(m => new
            {
                m.Id, m.StockItemId,
                Type = m.MovementType.ToString(),
                m.Quantity,
                Date = m.Date.ToString("yyyy-MM-dd"),
                m.Notes,
                m.BatchNumber
            }).ToList();

        return Ok(movements);
    }

    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        var today = DateTime.Today;
        return Ok(new
        {
            TotalItems = dbContext.StockItems.Count(),
            LowStockItems = dbContext.StockItems.Count(i => i.CurrentQuantity <= i.AlertThreshold),
            ExpiringIn30Days = dbContext.StockItems.Count(i => i.ExpirationDate.HasValue && i.ExpirationDate.Value <= today.AddDays(30)),
            TotalMovementsThisMonth = dbContext.StockMovements.Count(m => m.Date.Year == today.Year && m.Date.Month == today.Month),
            TotalSuppliers = dbContext.Suppliers.Count(),
            OpenOrders = dbContext.PurchaseOrders.Count(o => o.Status == PurchaseOrderStatus.Commandee)
        });
    }
}
