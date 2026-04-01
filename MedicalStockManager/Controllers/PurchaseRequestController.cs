using MedicalStockManager.Models;
using MedicalStockManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStockManager.Controllers;

[Authorize]
public class PurchaseRequestController(IPurchaseRequestService requestService, IAuditService auditService) : Controller
{
    private bool IsApprover => User.IsInRole(AppRole.Administrateur) || User.IsInRole(AppRole.GestionnaireStock);
    private string Username => User.Identity?.Name ?? "inconnu";

    [HttpGet]
    public IActionResult Index()
    {
        var model = requestService.GetIndex(Username, IsApprover);
        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(requestService.GetCreateModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(PurchaseRequestCreateViewModel input)
    {
        if (!ModelState.IsValid)
        {
            input.StockItems = requestService.GetCreateModel().StockItems;
            return View(input);
        }

        if (!requestService.CreateRequest(input, Username, out var errorMessage))
        {
            ModelState.AddModelError(string.Empty, errorMessage!);
            input.StockItems = requestService.GetCreateModel().StockItems;
            return View(input);
        }

        auditService.Log("Creation", "PurchaseRequest", null, $"Demande soumise par {Username}");
        TempData["SuccessMessage"] = "Votre demande d'achat a ete soumise et est en attente d'approbation.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var model = requestService.GetDetails(id, IsApprover);
        if (model is null) return NotFound();

        // Non-approvers can only see their own requests
        if (!IsApprover && model.Request.RequestedBy != Username)
            return Forbid();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Approve(ApproveRequestViewModel input)
    {
        if (!requestService.ApproveRequest(input, Username, out var errorMessage))
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Details), new { id = input.RequestId });
        }

        auditService.Log("Approbation", "PurchaseRequest", input.RequestId.ToString(), $"Approuve par {Username}");
        TempData["SuccessMessage"] = "Demande approuvee et bon de commande cree automatiquement.";
        return RedirectToAction(nameof(Details), new { id = input.RequestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Reject(RejectRequestViewModel input)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Le motif de rejet est obligatoire.";
            return RedirectToAction(nameof(Details), new { id = input.RequestId });
        }

        if (!requestService.RejectRequest(input, Username, out var errorMessage))
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Details), new { id = input.RequestId });
        }

        auditService.Log("Rejet", "PurchaseRequest", input.RequestId.ToString(), $"Rejete par {Username}: {input.RejectionReason}");
        TempData["SuccessMessage"] = "Demande rejetee.";
        return RedirectToAction(nameof(Index));
    }
}
