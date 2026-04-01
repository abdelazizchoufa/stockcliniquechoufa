using MedicalStockManager.Models;
using MedicalStockManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStockManager.Controllers;

[Authorize]
public class InventoryController(IInventoryService inventoryService, IAuditService auditService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = inventoryService.GetSessions();
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Create()
    {
        var model = inventoryService.GetCreateModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Create(InventoryCreateViewModel input, bool applyAdjustments = false)
    {
        var username = User.Identity?.Name ?? "inconnu";

        if (!inventoryService.CreateSession(input, username, applyAdjustments, out var errorMessage))
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Erreur lors de la creation de l'inventaire.");
            return View(input);
        }

        auditService.Log("Inventaire", "InventorySession", input.Title,
            applyAdjustments ? "Inventaire cree avec ajustements" : "Inventaire cree sans ajustements");
        TempData["SuccessMessage"] = applyAdjustments
            ? "Inventaire enregistre et stock ajuste."
            : "Inventaire enregistre sans modification du stock.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var session = inventoryService.GetSessionDetails(id);
        if (session is null) return NotFound();
        return View(session);
    }
}
