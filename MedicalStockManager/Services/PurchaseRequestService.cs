using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Services;

public class PurchaseRequestService(ApplicationDbContext dbContext) : IPurchaseRequestService
{
    public PurchaseRequestIndexViewModel GetIndex(string? username, bool isApprover)
    {
        var all = dbContext.PurchaseRequests
            .AsNoTracking()
            .Include(r => r.Lines)
            .OrderByDescending(r => r.RequestDate)
            .ThenByDescending(r => r.Id)
            .ToList();

        // Non-approvers only see their own requests
        if (!isApprover && username is not null)
            all = all.Where(r => r.RequestedBy == username).ToList();

        var toViewModel = (PurchaseRequest r) => new PurchaseRequestListItemViewModel
        {
            Id = r.Id,
            RequestNumber = r.RequestNumber,
            RequestedBy = r.RequestedBy,
            RequestDate = r.RequestDate,
            Status = r.Status,
            LineCount = r.Lines.Count,
            TotalEstimated = r.Lines.Sum(l => l.RequestedQuantity * l.EstimatedUnitPrice),
            ProcessedBy = r.ProcessedBy,
            ProcessedAt = r.ProcessedAt
        };

        return new PurchaseRequestIndexViewModel
        {
            Pending = all.Where(r => r.Status == PurchaseRequestStatus.EnAttente).Select(toViewModel).ToList(),
            Processed = all.Where(r => r.Status != PurchaseRequestStatus.EnAttente).Select(toViewModel).ToList(),
            TotalPending = all.Count(r => r.Status == PurchaseRequestStatus.EnAttente),
            TotalApproved = all.Count(r => r.Status == PurchaseRequestStatus.Approuvee || r.Status == PurchaseRequestStatus.ConvertieEnCommande),
            TotalRejected = all.Count(r => r.Status == PurchaseRequestStatus.Rejetee)
        };
    }

    public PurchaseRequestCreateViewModel GetCreateModel()
    {
        return new PurchaseRequestCreateViewModel
        {
            Lines = Enumerable.Range(0, 5).Select(_ => new PurchaseRequestLineInput()).ToList(),
            StockItems = GetStockItemSelectList()
        };
    }

    public bool CreateRequest(PurchaseRequestCreateViewModel input, string username, out string? errorMessage)
    {
        var validLines = input.Lines.Where(l => l.HasValue).ToList();

        if (!validLines.Any())
        {
            errorMessage = "Ajoutez au moins une ligne de demande.";
            return false;
        }

        var number = GenerateRequestNumber();

        var request = new PurchaseRequest
        {
            RequestNumber = number,
            RequestedBy = username,
            RequestDate = DateTime.Today,
            Status = PurchaseRequestStatus.EnAttente,
            Justification = input.Justification,
            Lines = validLines.Select(l => new PurchaseRequestLine
            {
                StockItemId = l.StockItemId,
                ItemLabel = l.ItemLabel,
                RequestedQuantity = l.RequestedQuantity,
                Unit = l.Unit,
                EstimatedUnitPrice = l.EstimatedUnitPrice,
                Notes = l.Notes
            }).ToList()
        };

        dbContext.PurchaseRequests.Add(request);
        dbContext.SaveChanges();
        errorMessage = null;
        return true;
    }

    public PurchaseRequestDetailsViewModel? GetDetails(int id, bool canApprove)
    {
        var request = dbContext.PurchaseRequests
            .AsNoTracking()
            .Include(r => r.Lines).ThenInclude(l => l.StockItem)
            .Include(r => r.LinkedOrder)
            .FirstOrDefault(r => r.Id == id);

        if (request is null) return null;

        return new PurchaseRequestDetailsViewModel
        {
            Request = request,
            TotalEstimated = request.Lines.Sum(l => l.EstimatedTotal),
            CanApprove = canApprove && request.Status == PurchaseRequestStatus.EnAttente,
            Suppliers = canApprove ? GetSupplierSelectList() : Array.Empty<SelectListItem>()
        };
    }

    public bool ApproveRequest(ApproveRequestViewModel input, string username, out string? errorMessage)
    {
        var request = dbContext.PurchaseRequests
            .Include(r => r.Lines).ThenInclude(l => l.StockItem)
            .FirstOrDefault(r => r.Id == input.RequestId);

        if (request is null) { errorMessage = "Demande introuvable."; return false; }
        if (request.Status != PurchaseRequestStatus.EnAttente) { errorMessage = "Cette demande a deja ete traitee."; return false; }
        if (!dbContext.Suppliers.Any(s => s.Id == input.SupplierId)) { errorMessage = "Fournisseur introuvable."; return false; }

        // Generate purchase order
        var orderNumber = $"CMD-{DateTime.Today:yyyyMMdd}-{GenerateOrderSuffix()}";

        var order = new PurchaseOrder
        {
            OrderNumber = orderNumber,
            SupplierId = input.SupplierId,
            OrderDate = DateTime.Today,
            ExpectedDeliveryDate = input.ExpectedDeliveryDate,
            Status = PurchaseOrderStatus.Commandee,
            Notes = $"Genere depuis demande {request.RequestNumber}. {input.Notes}".Trim(),
            Lines = request.Lines.Select(l => new PurchaseOrderLine
            {
                StockItemId = l.StockItemId ?? GetOrCreateStockItemId(l),
                QuantityOrdered = l.RequestedQuantity,
                UnitPrice = l.EstimatedUnitPrice
            }).ToList()
        };

        dbContext.PurchaseOrders.Add(order);
        dbContext.SaveChanges();

        request.Status = PurchaseRequestStatus.ConvertieEnCommande;
        request.ProcessedBy = username;
        request.ProcessedAt = DateTime.Now;
        request.LinkedOrderId = order.Id;
        dbContext.SaveChanges();

        errorMessage = null;
        return true;
    }

    public bool RejectRequest(RejectRequestViewModel input, string username, out string? errorMessage)
    {
        var request = dbContext.PurchaseRequests.FirstOrDefault(r => r.Id == input.RequestId);

        if (request is null) { errorMessage = "Demande introuvable."; return false; }
        if (request.Status != PurchaseRequestStatus.EnAttente) { errorMessage = "Cette demande a deja ete traitee."; return false; }

        request.Status = PurchaseRequestStatus.Rejetee;
        request.ProcessedBy = username;
        request.ProcessedAt = DateTime.Now;
        request.RejectionReason = input.RejectionReason;
        dbContext.SaveChanges();

        errorMessage = null;
        return true;
    }

    private int GetOrCreateStockItemId(PurchaseRequestLine line)
    {
        // Try to find existing stock item by label
        var existing = dbContext.StockItems.FirstOrDefault(i =>
            i.Name.ToLower() == line.ItemLabel.ToLower());

        if (existing is not null) return existing.Id;

        // Create a placeholder item
        var defaultServiceId = dbContext.Services
            .Where(s => s.Name == "Consultations")
            .Select(s => s.Id)
            .FirstOrDefault();

        if (defaultServiceId == 0)
        {
            defaultServiceId = dbContext.Services
                .Select(s => s.Id)
                .FirstOrDefault();
        }

        if (defaultServiceId == 0)
        {
            var fallbackService = new Service { Name = "Consultations" };
            dbContext.Services.Add(fallbackService);
            dbContext.SaveChanges();
            defaultServiceId = fallbackService.Id;
        }

        var item = new StockItem
        {
            Name = line.ItemLabel,
            Reference = $"REF-{DateTime.Now:yyyyMMddHHmmss}",
            ServiceId = defaultServiceId,
            CurrentQuantity = 0,
            AlertThreshold = 0,
            Unit = line.Unit
        };

        dbContext.StockItems.Add(item);
        dbContext.SaveChanges();
        return item.Id;
    }

    private string GenerateRequestNumber()
    {
        var count = dbContext.PurchaseRequests.Count(r => r.RequestDate.Year == DateTime.Today.Year) + 1;
        return $"DA-{DateTime.Today:yyyy}-{count:D4}";
    }

    private string GenerateOrderSuffix()
    {
        var count = dbContext.PurchaseOrders.Count(o => o.OrderDate == DateTime.Today) + 1;
        return count.ToString("D2");
    }

    private IReadOnlyList<SelectListItem> GetSupplierSelectList() =>
        dbContext.Suppliers.AsNoTracking().OrderBy(s => s.Name)
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            .ToList();

    private IReadOnlyList<SelectListItem> GetStockItemSelectList() =>
        dbContext.StockItems.AsNoTracking().OrderBy(i => i.Name)
            .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = $"{i.Name} ({i.Reference})" })
            .ToList();
}
