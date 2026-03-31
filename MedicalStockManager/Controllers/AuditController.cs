using MedicalStockManager.Models;
using MedicalStockManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStockManager.Controllers;

[Authorize(Roles = AppRole.Administrateur)]
public class AuditController(IAuditService auditService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = auditService.GetRecentLogs();
        return View(model);
    }
}
