using MedicalStockManager.Models;
using Microsoft.AspNetCore.Http;

namespace MedicalStockManager.Services;

public interface IOcrService
{
    Task<OcrDocumentResultViewModel> ExtractFromDocumentAsync(IFormFile file);
}
