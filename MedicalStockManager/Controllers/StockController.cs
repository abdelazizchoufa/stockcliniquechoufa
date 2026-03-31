using MedicalStockManager.Models;
using MedicalStockManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStockManager.Controllers;

[Authorize]
public class StockController(IStockService stockService, IAuditService auditService) : Controller
{
    [HttpGet]
    public IActionResult Index([FromQuery] StockFilterViewModel filter)
    {
        var model = stockService.GetStockIndex(filter);
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Create()
    {
        return View(stockService.GetCreateModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Create(StockItemFormViewModel item)
    {
        if (!ModelState.IsValid)
        {
            return View(item);
        }

        if (!stockService.AddItem(item, out var errorMessage))
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de creer l'article.");
            return View(item);
        }

        auditService.Log("Creation", "StockItem", item.Reference, $"Article {item.Name}");
        TempData["SuccessMessage"] = "L'article a ete ajoute au stock.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Edit(int id)
    {
        var model = stockService.GetEditModel(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Edit(StockItemFormViewModel item)
    {
        if (!ModelState.IsValid)
        {
            return View(item);
        }

        if (!stockService.UpdateItem(item, out var errorMessage))
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible de modifier l'article.");
            return View(item);
        }

        auditService.Log("Modification", "StockItem", item.Reference, $"Article {item.Name}");
        TempData["SuccessMessage"] = "L'article a ete mis a jour.";
        return RedirectToAction(nameof(Details), new { id = item.Id });
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Delete(int id)
    {
        var model = stockService.GetDeleteModel(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult ConfirmDelete(int id)
    {
        var deleteModel = stockService.GetDeleteModel(id);

        if (deleteModel is null)
        {
            return NotFound();
        }

        if (!stockService.DeleteItem(id, out var errorMessage))
        {
            TempData["ErrorMessage"] = errorMessage ?? "Impossible de supprimer l'article.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        auditService.Log("Suppression", "StockItem", deleteModel.Reference, $"Article {deleteModel.Name}");
        TempData["SuccessMessage"] = "L'article a ete supprime.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var model = stockService.GetItemDetails(id);

        if (model is null)
        {
            return NotFound();
        }

        model.CanEdit = User.IsInRole(AppRole.Administrateur) || User.IsInRole(AppRole.GestionnaireStock);
        return View(model);
    }

    [HttpGet]
    public IActionResult Print(int id)
    {
        var model = stockService.GetItemDetails(id);

        if (model is null)
        {
            return NotFound();
        }

        auditService.Log("Impression", "StockItem", model.Item.Reference, $"Fiche article {model.Item.Name}");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult AddMovement(StockMovementInputModel input)
    {
        if (!ModelState.IsValid)
        {
            var invalidModel = stockService.GetItemDetails(input.StockItemId);

            if (invalidModel is null)
            {
                return NotFound();
            }

            invalidModel.NewMovement = input;
            return View("Details", invalidModel);
        }

        if (!stockService.AddMovement(input, out var errorMessage))
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Impossible d'enregistrer le mouvement.");

            var failedModel = stockService.GetItemDetails(input.StockItemId);

            if (failedModel is null)
            {
                return NotFound();
            }

            failedModel.NewMovement = input;
            return View("Details", failedModel);
        }

        TempData["SuccessMessage"] = "Le mouvement de stock a ete enregistre.";
        auditService.Log("Mouvement", "StockItem", input.StockItemId.ToString(), $"{input.MovementType} quantite {input.Quantity}");
        return RedirectToAction(nameof(Details), new { id = input.StockItemId });
    }
}
