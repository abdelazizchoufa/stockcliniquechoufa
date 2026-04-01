using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Services;

public class MaterialRequestService(ApplicationDbContext dbContext, IStockService stockService, IAuditService auditService) : IMaterialRequestService
{
    public int CreateRequest(MaterialRequestFormViewModel input, string username)
    {
        var req = new MaterialRequest
        {
            RequestNumber = "REQ-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            RequestingServiceId = input.RequestingServiceId,
            RequestedByUsername = username,
            Notes = input.Notes,
            Status = RequestStatus.Brouillon,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var line in input.Lines)
        {
            if (line.RequestedQuantity > 0)
            {
                req.Lines.Add(new MaterialRequestLine
                {
                    StockItemId = line.StockItemId,
                    RequestedQuantity = line.RequestedQuantity
                });
            }
        }

        dbContext.MaterialRequests.Add(req);
        dbContext.SaveChanges();
        return req.Id;
    }

    public bool SubmitRequest(int requestId, out string? errorMessage)
    {
        var req = dbContext.MaterialRequests.FirstOrDefault(r => r.Id == requestId);
        if (req == null) { errorMessage = "Demande introuvable"; return false; }
        if (req.Status != RequestStatus.Brouillon) { errorMessage = "Statut invalide"; return false; }

        req.Status = RequestStatus.Soumis;
        dbContext.SaveChanges();
        auditService.Log("Soumission", "MaterialRequest", req.RequestNumber, "Demande soumise");
        errorMessage = null; return true;
    }

    public bool ApproveRequest(int requestId, string approverUsername, out string? errorMessage)
    {
        var req = dbContext.MaterialRequests.Include(r => r.Lines).Include(r => r.RequestingService).FirstOrDefault(r => r.Id == requestId);
        if (req == null) { errorMessage = "Demande introuvable"; return false; }
        if (req.Status != RequestStatus.Soumis) { errorMessage = "Seules les demandes soumises peuvent etre approuvees."; return false; }

        var destLocation = dbContext.Locations.FirstOrDefault(l => l.Name == req.RequestingService!.Name);
        if (destLocation == null)
        {
            // Auto create location for service if it doesn't exist
            destLocation = new Location { Name = req.RequestingService!.Name, IsCentral = false };
            dbContext.Locations.Add(destLocation);
            dbContext.SaveChanges();
        }

        foreach (var line in req.Lines)
        {
            var movementInput = new StockMovementInputModel
            {
                StockItemId = line.StockItemId,
                MovementType = MovementType.Transfert,
                Quantity = line.RequestedQuantity,
                Date = DateTime.Today,
                Notes = $"Livraison pour {req.RequestNumber}",
                LocationId = dbContext.Locations.First(l => l.IsCentral).Id,
                DestinationLocationId = destLocation.Id
            };

            // Call StockService FEFO execution.
            if (!stockService.AddMovement(movementInput, out var moveError))
            {
                errorMessage = $"Erreur sur l'article {line.StockItemId}: {moveError}";
                return false;
            }
            
            line.ApprovedQuantity = line.RequestedQuantity;
        }

        req.Status = RequestStatus.Livre;
        req.ProcessedByUsername = approverUsername;
        dbContext.SaveChanges();
        
        auditService.Log("Approbation", "MaterialRequest", req.RequestNumber, "Demande livree via FEFO");
        errorMessage = null; return true;
    }

    public bool RejectRequest(int requestId, string rejecterUsername, string reason, out string? errorMessage)
    {
        var req = dbContext.MaterialRequests.FirstOrDefault(r => r.Id == requestId);
        if (req == null) { errorMessage = "Demande introuvable"; return false; }
        
        req.Status = RequestStatus.Rejete;
        req.ProcessedByUsername = rejecterUsername;
        req.RejectionReason = reason;
        
        dbContext.SaveChanges();
        auditService.Log("Rejet", "MaterialRequest", req.RequestNumber, "Demande rejetee");
        errorMessage = null; return true;
    }

    public MaterialRequest? GetRequestDetails(int id)
    {
        return dbContext.MaterialRequests
            .Include(r => r.RequestingService)
            .Include(r => r.Lines)
                .ThenInclude(l => l.StockItem)
            .FirstOrDefault(r => r.Id == id);
    }

    public IReadOnlyList<MaterialRequest> GetMyRequests(string username)
    {
        return dbContext.MaterialRequests
            .Include(r => r.RequestingService)
            .Where(r => r.RequestedByUsername == username)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public IReadOnlyList<MaterialRequest> GetPendingRequests()
    {
        return dbContext.MaterialRequests
            .Include(r => r.RequestingService)
            .Where(r => r.Status == RequestStatus.Soumis)
            .OrderBy(r => r.CreatedAt)
            .ToList();
    }

    public IReadOnlyList<MaterialRequest> GetCompletedRequests()
    {
        return dbContext.MaterialRequests
            .Include(r => r.RequestingService)
            .Where(r => r.Status == RequestStatus.Livre || r.Status == RequestStatus.Rejete)
            .OrderByDescending(r => r.CreatedAt)
            .Take(50)
            .ToList();
    }
}
