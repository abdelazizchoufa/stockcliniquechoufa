using MedicalStockManager.Models;
using MedicalStockManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStockManager.Controllers;

[Route("api/scan")]
[ApiController]
// Pour un vrai cas PWA, activer [Authorize(AuthenticationSchemes = "Bearer")] avec JWT.
// Ici, en MVC par defaut, on permet l'appel des endpoints ou on ajoute une authentification basee sur des cles API.
public class StockApiController(IStockService stockService) : ControllerBase
{
    public class ScanOutDto
    {
        public string Reference { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int? DestinationLocationId { get; set; }
        public string? Notes { get; set; }
    }

    [HttpPost("out")]
    public IActionResult ScanOut([FromBody] ScanOutDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reference) || dto.Quantity <= 0)
        {
            return BadRequest(new { Error = "Reference et Quantité superieure a 0 obligatoires." });
        }

        // Trouver le StockItemId grace a la reference
        var filter = new StockFilterViewModel { SearchTerm = dto.Reference };
        var index = stockService.GetStockIndex(filter);
        var item = index.Items.FirstOrDefault(i => i.Reference == dto.Reference);

        if (item == null)
            return NotFound(new { Error = "Article introuvable." });

        var input = new StockMovementInputModel
        {
            StockItemId = item.Id,
            Quantity = dto.Quantity,
            Date = DateTime.Today,
            Notes = dto.Notes ?? "Scan Mobile",
            // Si pas de destination -> Sortie (consommation directe). Si destination -> Transfert FEFO
            MovementType = dto.DestinationLocationId.HasValue ? MovementType.Transfert : MovementType.Sortie,
            DestinationLocationId = dto.DestinationLocationId
        };

        if (stockService.AddMovement(input, out var errorMessage))
        {
            return Ok(new { Message = "Mouvement valide avec succes (Algorithme FEFO applique).", ItemName = item.Name });
        }

        return BadRequest(new { Error = errorMessage });
    }
}
