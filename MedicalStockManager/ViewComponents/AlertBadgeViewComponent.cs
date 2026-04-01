using MedicalStockManager.Data;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStockManager.ViewComponents;

public class AlertBadgeViewComponent(ApplicationDbContext dbContext) : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var today = DateTime.Today;
        var expiringLimit = today.AddDays(30);

        var lowStockCount = dbContext.StockItems
            .Count(i => i.CurrentQuantity <= i.AlertThreshold);

        var expiringCount = dbContext.StockItems
            .Count(i => i.ExpirationDate.HasValue && i.ExpirationDate.Value <= expiringLimit);

        var total = lowStockCount + expiringCount;
        return View(total);
    }
}
