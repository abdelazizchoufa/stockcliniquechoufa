using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStockManager.ViewComponents;

public class PendingRequestBadgeViewComponent(ApplicationDbContext dbContext) : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        // Only show to approvers
        if (!UserClaimsPrincipal.IsInRole(AppRole.Administrateur) &&
            !UserClaimsPrincipal.IsInRole(AppRole.GestionnaireStock))
            return Content(string.Empty);

        var count = dbContext.PurchaseRequests
            .Count(r => r.Status == PurchaseRequestStatus.EnAttente);

        return View(count);
    }
}
