using MedicalStockManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace MedicalStockManager.Controllers;

[Authorize]
public class ReportsController(IReportService reportService, IAuditService auditService) : Controller
{
    [HttpGet]
    public IActionResult Consumption(DateTime? startDate, DateTime? endDate)
    {
        var model = reportService.GetConsumptionReport(startDate, endDate);
        return View(model);
    }

    [HttpGet]
    public IActionResult PrintConsumption(DateTime? startDate, DateTime? endDate)
    {
        var model = reportService.GetConsumptionReport(startDate, endDate);
        auditService.Log(
            "Impression",
            "ConsumptionReport",
            null,
            $"Periode {model.Filter.StartDate:dd/MM/yyyy} - {model.Filter.EndDate:dd/MM/yyyy}");
        return View(model);
    }

    [HttpGet]
    public IActionResult ExportConsumption(DateTime? startDate, DateTime? endDate)
    {
        var model = reportService.GetConsumptionReport(startDate, endDate);
        var builder = new StringBuilder();
        builder.AppendLine("Service,Article,Reference,QuantiteSortie,Mouvements");

        foreach (var item in model.TopItems)
        {
            builder.AppendLine($"{item.ServiceName},\"{item.ItemName}\",{item.Reference},{item.TotalQuantityOut},{item.MovementCount}");
        }

        auditService.Log(
            "Export",
            "ConsumptionReport",
            null,
            $"Periode {model.Filter.StartDate:dd/MM/yyyy} - {model.Filter.EndDate:dd/MM/yyyy}");

        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
        var fileName = $"rapport-consommation-{model.Filter.StartDate:yyyyMMdd}-{model.Filter.EndDate:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }

    [HttpGet]
    public IActionResult Analytics()
    {
        var model = reportService.GetAnalytics();
        return View(model);
    }
}
