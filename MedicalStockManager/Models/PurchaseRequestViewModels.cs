using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalStockManager.Models;

public class PurchaseRequestLineInput
{
    public int? StockItemId { get; set; }

    [Required(ErrorMessage = "La designation est obligatoire.")]
    [MaxLength(150)]
    [Display(Name = "Designation")]
    public string ItemLabel { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "La quantite doit etre au moins 1.")]
    [Display(Name = "Quantite")]
    public int RequestedQuantity { get; set; } = 1;

    [MaxLength(30)]
    [Display(Name = "Unite")]
    public string Unit { get; set; } = "unite";

    [Range(0, double.MaxValue)]
    [Display(Name = "Prix estime (DA)")]
    public decimal EstimatedUnitPrice { get; set; }

    [MaxLength(200)]
    [Display(Name = "Justification")]
    public string? Notes { get; set; }

    public bool HasValue => !string.IsNullOrWhiteSpace(ItemLabel);
}

public class PurchaseRequestCreateViewModel
{
    [Required(ErrorMessage = "La justification est obligatoire.")]
    [MaxLength(300)]
    [Display(Name = "Justification globale")]
    public string Justification { get; set; } = string.Empty;

    public List<PurchaseRequestLineInput> Lines { get; set; } = new();
    public IReadOnlyList<SelectListItem> StockItems { get; set; } = Array.Empty<SelectListItem>();
}

public class PurchaseRequestListItemViewModel
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public PurchaseRequestStatus Status { get; set; }
    public int LineCount { get; set; }
    public decimal TotalEstimated { get; set; }
    public string? ProcessedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class PurchaseRequestIndexViewModel
{
    public IReadOnlyList<PurchaseRequestListItemViewModel> Pending { get; set; } = Array.Empty<PurchaseRequestListItemViewModel>();
    public IReadOnlyList<PurchaseRequestListItemViewModel> Processed { get; set; } = Array.Empty<PurchaseRequestListItemViewModel>();
    public int TotalPending { get; set; }
    public int TotalApproved { get; set; }
    public int TotalRejected { get; set; }
}

public class PurchaseRequestDetailsViewModel
{
    public PurchaseRequest Request { get; set; } = new();
    public decimal TotalEstimated { get; set; }
    public bool CanApprove { get; set; }
    public IReadOnlyList<SelectListItem> Suppliers { get; set; } = Array.Empty<SelectListItem>();
}

public class ApproveRequestViewModel
{
    public int RequestId { get; set; }
    public int SupplierId { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; } = DateTime.Today.AddDays(14);
    public string? Notes { get; set; }
}

public class RejectRequestViewModel
{
    [Required(ErrorMessage = "Le motif de rejet est obligatoire.")]
    [MaxLength(300)]
    [Display(Name = "Motif du rejet")]
    public string RejectionReason { get; set; } = string.Empty;
    public int RequestId { get; set; }
}
