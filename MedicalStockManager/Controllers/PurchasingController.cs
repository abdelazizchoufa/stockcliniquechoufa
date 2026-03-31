using MedicalStockManager.Models;
using MedicalStockManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStockManager.Controllers;

[Authorize]
public class PurchasingController(IPurchasingService purchasingService, IAuditService auditService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = purchasingService.GetOverview();
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult CreateSupplier()
    {
        return View(purchasingService.GetSupplierCreateModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult CreateSupplier(SupplierFormViewModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        if (!purchasingService.AddSupplier(input, out var errorMessage))
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible d'ajouter le fournisseur.");
            return View(input);
        }

        auditService.Log("Creation", "Supplier", input.Name, "Fournisseur cree");
        TempData["SuccessMessage"] = "Le fournisseur a ete ajoute.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult EditSupplier(int id)
    {
        var model = purchasingService.GetSupplierEditModel(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult EditSupplier(SupplierFormViewModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        if (!purchasingService.UpdateSupplier(input, out var errorMessage))
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de modifier le fournisseur.");
            return View(input);
        }

        auditService.Log("Modification", "Supplier", input.Name, "Fournisseur mis a jour");
        TempData["SuccessMessage"] = "Le fournisseur a ete mis a jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult DeleteSupplier(int id)
    {
        var model = purchasingService.GetSupplierDeleteModel(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult ConfirmDeleteSupplier(int id)
    {
        var deleteModel = purchasingService.GetSupplierDeleteModel(id);

        if (deleteModel is null)
        {
            return NotFound();
        }

        if (!purchasingService.DeleteSupplier(id, out var errorMessage))
        {
            TempData["ErrorMessage"] = errorMessage ?? "Impossible de supprimer le fournisseur.";
            return RedirectToAction(nameof(DeleteSupplier), new { id });
        }

        auditService.Log("Suppression", "Supplier", deleteModel.Name, "Fournisseur supprime");
        TempData["SuccessMessage"] = "Le fournisseur a ete supprime.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Create()
    {
        var model = purchasingService.GetCreateModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Create(PurchaseOrderCreateViewModel input)
    {
        if (!ModelState.IsValid)
        {
            var invalidModel = purchasingService.PrepareCreateModel(input);
            return View(invalidModel);
        }

        if (!purchasingService.CreateOrder(input, out var errorMessage))
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de creer la commande.");
            var failedModel = purchasingService.PrepareCreateModel(input);
            return View(failedModel);
        }

        TempData["SuccessMessage"] = "La commande fournisseur a ete enregistree.";
        auditService.Log("Creation", "PurchaseOrder", input.OrderNumber, "Commande fournisseur creee");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var model = purchasingService.GetDetails(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Print(int id)
    {
        var model = purchasingService.GetPrintDetails(id);

        if (model is null)
        {
            return NotFound();
        }

        auditService.Log("Impression", "PurchaseOrder", id.ToString(), "Impression commande fournisseur");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Receive(int id)
    {
        if (!purchasingService.ReceiveOrder(id, out var errorMessage))
        {
            TempData["ErrorMessage"] = errorMessage ?? "Impossible de receptionner la commande.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["SuccessMessage"] = "La commande a ete receptionnee et le stock a ete mis a jour.";
        auditService.Log("Reception", "PurchaseOrder", id.ToString(), "Commande receptionnee");
        return RedirectToAction(nameof(Details), new { id });
    }
}
