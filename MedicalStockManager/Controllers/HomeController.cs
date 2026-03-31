using MedicalStockManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStockManager.Controllers;

[Authorize]
public class HomeController(IStockService stockService) : Controller
{
    public IActionResult Index()
    {
        var model = stockService.GetDashboard();
        return View(model);
    }

    public IActionResult Error()
    {
        return View();
    }
}
