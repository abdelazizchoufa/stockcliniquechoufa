using MedicalStockManager.Models;
using MedicalStockManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStockManager.Controllers;

[Authorize]
public class MaterialRequestController(IMaterialRequestService requestService, IStockService stockService) : Controller
{
    public IActionResult Index()
    {
        var username = User.Identity?.Name ?? "";
        if (User.IsInRole(AppRole.Administrateur) || User.IsInRole(AppRole.GestionnaireStock))
        {
            var pending = requestService.GetPendingRequests();
            var completed = requestService.GetCompletedRequests();
            ViewBag.Pending = pending;
            ViewBag.Completed = completed;
            return View("AdminIndex");
        }
        else
        {
            var myRequests = requestService.GetMyRequests(username);
            return View(myRequests);
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        var form = new MaterialRequestFormViewModel();
        ViewBag.Services = stockService.GetServiceDirectory().Services;
        ViewBag.Items = stockService.GetStockIndex(new StockFilterViewModel()).Items;
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(MaterialRequestFormViewModel form)
    {
        var username = User.Identity?.Name ?? "";
        if (form.RequestingServiceId == 0)
        {
            ModelState.AddModelError("", "Le service demandeur est obligatoire.");
        }

        form.Lines = form.Lines.Where(l => l.RequestedQuantity > 0).ToList();
        if (!form.Lines.Any())
        {
            ModelState.AddModelError("", "Vous devez commander au moins 1 article.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Services = stockService.GetServiceDirectory().Services;
            ViewBag.Items = stockService.GetStockIndex(new StockFilterViewModel()).Items;
            return View(form);
        }

        var reqId = requestService.CreateRequest(form, username);
        return RedirectToAction(nameof(Details), new { id = reqId });
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var req = requestService.GetRequestDetails(id);
        if (req == null) return NotFound();
        return View(req);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit(int id)
    {
        if (requestService.SubmitRequest(id, out var error))
        {
            TempData["SuccessMessage"] = "La demande a ete soumise pour approbation.";
        }
        else
        {
            TempData["ErrorMessage"] = error;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Approve(int id)
    {
        var username = User.Identity?.Name ?? "";
        if (requestService.ApproveRequest(id, username, out var error))
        {
            TempData["SuccessMessage"] = "Commande validee et livree avec succes via l'algorithme FEFO.";
        }
        else
        {
            TempData["ErrorMessage"] = "Erreur de livraison : " + error;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{AppRole.Administrateur},{AppRole.GestionnaireStock}")]
    public IActionResult Reject(int id, string reason)
    {
        var username = User.Identity?.Name ?? "";
        if (requestService.RejectRequest(id, username, reason, out var error))
        {
            TempData["SuccessMessage"] = "La commande a ete rejetee.";
        }
        else
        {
            TempData["ErrorMessage"] = error;
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
