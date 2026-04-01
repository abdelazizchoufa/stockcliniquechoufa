using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using MedicalStockManager.Models;
using Tesseract;
using UglyToad.PdfPig;

namespace MedicalStockManager.Services;

public class OcrService(IWebHostEnvironment environment) : IOcrService
{
    private static readonly string[] SupportedImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff", ".webp"];
    private static readonly string[] SupportedPdfExtensions = [".pdf"];
    private static readonly Regex LedgerEntryRegex = new(
        @"(?s)(?<date>\d{2}/\d{2}/\d{4})\s*(?<ref>\d{1,4}/\d{2})?\s*(?<payment>Esp[eè]ce|A\s*Terme|Ch[eè]que)\s*(?<amount>[0-9\s,\.]+?)\s*0[.,]00\s*(?<balance>-?[0-9\s,\.]+)\s*SOLDE(?<body>.*?)(?=(?:\d{2}/\d{2}/\d{4}\s*(?:\d{1,4}/\d{2})?\s*(?:Esp[eè]ce|A\s*Terme|Ch[eè]que))|\z)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task<OcrDocumentResultViewModel> ExtractFromDocumentAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var result = new OcrDocumentResultViewModel
        {
            FileName = file.FileName,
            MimeType = file.ContentType ?? string.Empty
        };

        if (SupportedPdfExtensions.Contains(extension))
        {
            var pdfTempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");
            try
            {
                await using (var stream = new FileStream(pdfTempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(stream);
                }

                using var document = PdfDocument.Open(pdfTempFile);
                var pages = document.GetPages().ToList();
                result.RawText = NormalizeText(string.Join(Environment.NewLine + Environment.NewLine, pages.Select(p => p.Text)).Trim());

                if (string.IsNullOrWhiteSpace(result.RawText))
                {
                    result.Warnings = ["Aucun texte detecte dans le PDF. Si le PDF est scanne, convertissez-le en image (JPG/PNG) pour l'OCR."];
                }

                EnrichStructuredFields(result);
                return result;
            }
            finally
            {
                if (File.Exists(pdfTempFile))
                {
                    File.Delete(pdfTempFile);
                }
            }
        }

        if (!SupportedImageExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Format non supporte. Utilisez une image (JPG, PNG, WEBP, TIFF, BMP) ou un PDF.");
        }

        var tessDataPath = ResolveTessDataPath();
        if (!File.Exists(Path.Combine(tessDataPath, "fra.traineddata")) &&
            !File.Exists(Path.Combine(tessDataPath, "eng.traineddata")))
        {
            throw new InvalidOperationException("Fichiers OCR manquants. Ajoutez 'fra.traineddata' et/ou 'eng.traineddata' dans le dossier 'tessdata'.");
        }

        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");
        try
        {
            await using (var stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            var language = BuildLanguageString(tessDataPath);
            using var engine = new TesseractEngine(tessDataPath, language, EngineMode.Default);
            using var image = Pix.LoadFromFile(tempFile);
            using var page = engine.Process(image);

            result.RawText = NormalizeText(page.GetText()?.Trim() ?? string.Empty);
            EnrichStructuredFields(result);
            return result;
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    private string ResolveTessDataPath()
    {
        var contentPath = Path.Combine(environment.ContentRootPath, "tessdata");
        if (Directory.Exists(contentPath))
        {
            return contentPath;
        }

        var appBasePath = Path.Combine(AppContext.BaseDirectory, "tessdata");
        if (Directory.Exists(appBasePath))
        {
            return appBasePath;
        }

        Directory.CreateDirectory(contentPath);
        return contentPath;
    }

    private static string BuildLanguageString(string tessDataPath)
    {
        var langs = new List<string>();
        if (File.Exists(Path.Combine(tessDataPath, "fra.traineddata")))
        {
            langs.Add("fra");
        }
        if (File.Exists(Path.Combine(tessDataPath, "eng.traineddata")))
        {
            langs.Add("eng");
        }
        return string.Join("+", langs);
    }

    private static void EnrichStructuredFields(OcrDocumentResultViewModel result)
    {
        var text = result.RawText;
        if (string.IsNullOrWhiteSpace(text))
        {
            result.Warnings = ["Aucun texte detecte. Essayez une image plus nette ou mieux cadree."];
            return;
        }

        result.DocumentNumber = MatchFirst(text,
            @"(?im)(?:facture|invoice|bl|bon\s+de\s+livraison)\s*(?:n[°o]\.?\s*|num(?:ero)?\s*:?\s*)([A-Z0-9\-_/]+)",
            @"(?im)\b(?:n[°o]\.?|num(?:ero)?)\s*[:\-]?\s*([A-Z0-9\-_/]{4,})");

        result.SupplierName = MatchFirst(text,
            @"(?im)(?:fournisseur|supplier)\s*[:\-]\s*(.+)$",
            @"(?im)\b(?:SARL|SPA|EURL|SAS|SA)\s+[A-Z0-9][A-Z0-9\s\-]+");

        result.DocumentDate = ParseDate(MatchFirst(text,
            @"(?im)(?:date(?:\s+facture)?|invoice\s+date|date\s+bl)\s*[:\-]?\s*([0-3]?\d[\/\-.][01]?\d[\/\-.](?:20)?\d{2})",
            @"(?im)\b([0-3]?\d[\/\-.][01]?\d[\/\-.](?:20)?\d{2})\b"));

        result.TotalAmount = ParseAmount(MatchFirst(text,
            @"(?im)(?:total\s*(?:ttc|t\.?t\.?c\.?)?|montant\s*total|net\s*a\s*payer)\s*[:\-]?\s*([0-9][0-9\.\,\s]+)",
            @"(?im)([0-9][0-9\.\,\s]{3,})\s*(?:da|dzd|eur|€|\$)"));

        result.LedgerEntries = ExtractLedgerEntries(text);
    }

    private static IReadOnlyList<OcrLedgerEntryViewModel> ExtractLedgerEntries(string text)
    {
        var matches = LedgerEntryRegex.Matches(text);
        if (matches.Count == 0)
        {
            return Array.Empty<OcrLedgerEntryViewModel>();
        }

        var entries = new List<OcrLedgerEntryViewModel>(matches.Count);
        foreach (Match match in matches)
        {
            var body = NormalizeWhitespace(match.Groups["body"].Value);
            var reference = CleanReference(match.Groups["ref"].Value);

            entries.Add(new OcrLedgerEntryViewModel
            {
                Date = ParseDate(match.Groups["date"].Value),
                ReferenceNumber = reference,
                BlNumber = reference,
                FactureNumber = reference,
                PaymentMethod = NormalizeWhitespace(match.Groups["payment"].Value),
                Amount = ParseAmount(match.Groups["amount"].Value),
                Balance = ParseAmount(match.Groups["balance"].Value),
                ArticleLines = ExtractArticleLines(body)
            });
        }

        return entries;
    }

    private static IReadOnlyList<string> ExtractArticleLines(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return Array.Empty<string>();
        }

        var cleaned = body
            .Replace("N° Facture", " ")
            .Replace("N° BL", " ")
            .Replace("DateArticlePrix", " ")
            .Replace("Date Article Prix", " ")
            .Replace("Total(HT)", " ")
            .Replace("MontantVersé", " ")
            .Replace("Reglement", " ")
            .Replace("Règlement", " ");

        return Regex.Matches(
                cleaned,
                @"(?<=\d{2})([A-Za-zµ][A-Za-z0-9µ\-\+\(\)\*\/\s\.]{4,}?)(?=\d{1,3}(?:\s\d{3})*[\.,]\d{2}\d)",
                RegexOptions.CultureInvariant)
            .Select(m => NormalizeWhitespace(m.Groups[1].Value))
            .Where(n => n.Length >= 5 && !n.StartsWith("SOLDE", StringComparison.OrdinalIgnoreCase))
            .Take(8)
            .ToList();
    }

    private static string? MatchFirst(string text, params string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.CultureInvariant);
            if (match.Success && match.Groups.Count > 1)
            {
                var value = match.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return null;
    }

    private static DateTime? ParseDate(string? rawDate)
    {
        if (string.IsNullOrWhiteSpace(rawDate))
        {
            return null;
        }

        var normalized = rawDate.Replace('.', '/').Replace('-', '/');
        var formats = new[] { "d/M/yy", "d/M/yyyy", "dd/MM/yy", "dd/MM/yyyy" };
        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(normalized, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
        }

        return null;
    }

    private static decimal? ParseAmount(string? rawAmount)
    {
        if (string.IsNullOrWhiteSpace(rawAmount))
        {
            return null;
        }

        var compact = rawAmount.Replace(" ", string.Empty);
        if (decimal.TryParse(compact, NumberStyles.Number, CultureInfo.GetCultureInfo("fr-FR"), out var frValue))
        {
            return frValue;
        }
        if (decimal.TryParse(compact, NumberStyles.Number, CultureInfo.InvariantCulture, out var invValue))
        {
            return invValue;
        }
        return null;
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var decoded = WebUtility.HtmlDecode(text);
        decoded = decoded.Replace('\u00A0', ' ');
        decoded = Regex.Replace(decoded, @"\s*Page\s+\d+\s*/\s*\d+\s*", Environment.NewLine, RegexOptions.IgnoreCase);
        decoded = Regex.Replace(decoded, @"(Esp[eè]ce|A\s*Terme|Ch[eè]que)", " $1 ", RegexOptions.IgnoreCase);
        decoded = Regex.Replace(decoded, @"(\d{2}/\d{2}/\d{4})(\d{1,4}/\d{2})", "$1 $2");
        decoded = Regex.Replace(decoded, @"(\d{2}/\d{2}/\d{4})(Esp[eè]ce|A\s*Terme|Ch[eè]que)", "$1 $2", RegexOptions.IgnoreCase);
        decoded = Regex.Replace(decoded, @"[ \t]+", " ");
        return decoded.Trim();
    }

    private static string NormalizeWhitespace(string value)
    {
        return Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim();
    }

    private static string? CleanReference(string? value)
    {
        var cleaned = NormalizeWhitespace(value ?? string.Empty);
        return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
    }
}
