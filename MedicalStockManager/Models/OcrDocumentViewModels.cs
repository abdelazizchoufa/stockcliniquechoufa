using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MedicalStockManager.Models;

public class OcrDocumentInputViewModel
{
    [Required(ErrorMessage = "Veuillez selectionner un fichier.")]
    [Display(Name = "Facture / BL")]
    public IFormFile? File { get; set; }
}

public class OcrDocumentResultViewModel
{
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string? SupplierName { get; set; }
    public DateTime? DocumentDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();
    public IReadOnlyList<OcrLedgerEntryViewModel> LedgerEntries { get; set; } = Array.Empty<OcrLedgerEntryViewModel>();
}

public class OcrLedgerEntryViewModel
{
    public DateTime? Date { get; set; }
    public string? BlNumber { get; set; }
    public string? FactureNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public decimal? Balance { get; set; }
    public IReadOnlyList<string> ArticleLines { get; set; } = Array.Empty<string>();
}

public class OcrDocumentPageViewModel
{
    public OcrDocumentInputViewModel Input { get; set; } = new();
    public OcrDocumentResultViewModel? Result { get; set; }
}
