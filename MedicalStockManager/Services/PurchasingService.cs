using MedicalStockManager.Data;
using MedicalStockManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Services;

public class PurchasingService(ApplicationDbContext dbContext) : IPurchasingService
{
    public PurchaseOrdersIndexViewModel GetOverview()
    {
        var orderEntities = dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Supplier)
            .Include(order => order.Lines)
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.Id)
            .ToList();

        var orders = orderEntities
            .Select(order => new PurchaseOrderListItemViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                SupplierName = order.Supplier != null ? order.Supplier.Name : string.Empty,
                OrderDate = order.OrderDate,
                ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                Status = order.Status,
                LineCount = order.Lines.Count,
                TotalAmount = order.Lines.Sum(line => line.QuantityOrdered * line.UnitPrice)
            })
            .ToList();

        var suppliers = dbContext.Suppliers
            .AsNoTracking()
            .Include(supplier => supplier.PurchaseOrders)
            .OrderBy(supplier => supplier.Name)
            .Select(supplier => new SupplierSummaryViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactName = supplier.ContactName,
                Phone = supplier.Phone,
                Email = supplier.Email,
                OrderCount = supplier.PurchaseOrders.Count,
                LastOrderDate = supplier.PurchaseOrders
                    .OrderByDescending(order => order.OrderDate)
                    .Select(order => (DateTime?)order.OrderDate)
                    .FirstOrDefault()
            })
            .ToList();

        var totalOrderedAmount = orderEntities.Sum(order => order.Lines.Sum(line => line.QuantityOrdered * line.UnitPrice));
        var totalReceivedAmount = orderEntities
            .Where(order => order.Status == PurchaseOrderStatus.Recue)
            .Sum(order => order.Lines.Sum(line => line.QuantityOrdered * line.UnitPrice));
        var topSuppliers = orderEntities
            .Where(order => order.Supplier is not null)
            .GroupBy(order => order.Supplier!.Name)
            .Select(group => new SupplierSpendViewModel
            {
                SupplierName = group.Key,
                TotalAmount = group.Sum(order => order.Lines.Sum(line => line.QuantityOrdered * line.UnitPrice)),
                OrderCount = group.Count()
            })
            .OrderByDescending(group => group.TotalAmount)
            .Take(5)
            .ToList();

        return new PurchaseOrdersIndexViewModel
        {
            Orders = orders,
            Suppliers = suppliers,
            FinanceSummary = new PurchaseFinanceSummaryViewModel
            {
                TotalOrderedAmount = totalOrderedAmount,
                TotalReceivedAmount = totalReceivedAmount,
                PendingAmount = totalOrderedAmount - totalReceivedAmount,
                OrdersThisMonth = orderEntities.Count(order =>
                    order.OrderDate.Year == DateTime.Today.Year &&
                    order.OrderDate.Month == DateTime.Today.Month),
                AverageOrderAmount = orderEntities.Count == 0 ? 0 : totalOrderedAmount / orderEntities.Count
            },
            TopSuppliers = topSuppliers
        };
    }

    public SupplierFormViewModel GetSupplierCreateModel()
    {
        return new SupplierFormViewModel();
    }

    public SupplierFormViewModel? GetSupplierEditModel(int id)
    {
        return dbContext.Suppliers
            .AsNoTracking()
            .Where(supplier => supplier.Id == id)
            .Select(supplier => new SupplierFormViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactName = supplier.ContactName,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address
            })
            .FirstOrDefault();
    }

    public SupplierDeleteViewModel? GetSupplierDeleteModel(int id)
    {
        return dbContext.Suppliers
            .AsNoTracking()
            .Include(supplier => supplier.PurchaseOrders)
            .Where(supplier => supplier.Id == id)
            .Select(supplier => new SupplierDeleteViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactName = supplier.ContactName,
                OrderCount = supplier.PurchaseOrders.Count,
                CanDelete = supplier.PurchaseOrders.Count == 0,
                BlockingReason = supplier.PurchaseOrders.Count == 0
                    ? null
                    : "Ce fournisseur est deja lie a des commandes et ne peut pas etre supprime."
            })
            .FirstOrDefault();
    }

    public bool AddSupplier(SupplierFormViewModel input, out string? errorMessage)
    {
        if (dbContext.Suppliers.Any(supplier => supplier.Name == input.Name))
        {
            errorMessage = "Ce fournisseur existe deja.";
            return false;
        }

        dbContext.Suppliers.Add(new Supplier
        {
            Name = input.Name,
            ContactName = input.ContactName,
            Phone = input.Phone,
            Email = input.Email,
            Address = input.Address
        });
        dbContext.SaveChanges();
        errorMessage = null;
        return true;
    }

    public bool UpdateSupplier(SupplierFormViewModel input, out string? errorMessage)
    {
        var supplier = dbContext.Suppliers.FirstOrDefault(item => item.Id == input.Id);

        if (supplier is null)
        {
            errorMessage = "Fournisseur introuvable.";
            return false;
        }

        if (dbContext.Suppliers.Any(item => item.Name == input.Name && item.Id != input.Id))
        {
            errorMessage = "Un autre fournisseur utilise deja ce nom.";
            return false;
        }

        supplier.Name = input.Name;
        supplier.ContactName = input.ContactName;
        supplier.Phone = input.Phone;
        supplier.Email = input.Email;
        supplier.Address = input.Address;

        dbContext.SaveChanges();
        errorMessage = null;
        return true;
    }

    public bool DeleteSupplier(int id, out string? errorMessage)
    {
        var supplier = dbContext.Suppliers.FirstOrDefault(item => item.Id == id);

        if (supplier is null)
        {
            errorMessage = "Fournisseur introuvable.";
            return false;
        }

        if (dbContext.PurchaseOrders.Any(order => order.SupplierId == id))
        {
            errorMessage = "Ce fournisseur est deja utilise dans des commandes.";
            return false;
        }

        dbContext.Suppliers.Remove(supplier);
        dbContext.SaveChanges();
        errorMessage = null;
        return true;
    }

    public PurchaseOrderCreateViewModel GetCreateModel()
    {
        return new PurchaseOrderCreateViewModel
        {
            OrderNumber = $"CMD-{DateTime.Today:yyyyMMdd}-01",
            OrderDate = DateTime.Today,
            Suppliers = GetSupplierSelectList(),
            StockItems = GetStockItemSelectList(),
            Lines = CreateDefaultLines()
        };
    }

    public PurchaseOrderCreateViewModel PrepareCreateModel(PurchaseOrderCreateViewModel input)
    {
        input.Suppliers = GetSupplierSelectList();
        input.StockItems = GetStockItemSelectList();
        input.Lines ??= [];

        while (input.Lines.Count < 5)
        {
            input.Lines.Add(new PurchaseOrderLineInputModel());
        }

        return input;
    }

    public PurchaseOrderDetailsViewModel? GetDetails(int id)
    {
        var order = dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(purchaseOrder => purchaseOrder.Supplier)
            .Include(purchaseOrder => purchaseOrder.Lines)
            .ThenInclude(line => line.StockItem)
            .FirstOrDefault(purchaseOrder => purchaseOrder.Id == id);

        if (order is null)
        {
            return null;
        }

        return new PurchaseOrderDetailsViewModel
        {
            Order = order,
            TotalAmount = order.Lines.Sum(line => line.QuantityOrdered * line.UnitPrice),
            TotalQuantity = order.Lines.Sum(line => line.QuantityOrdered)
        };
    }

    public PurchaseOrderDetailsViewModel? GetPrintDetails(int id)
    {
        return GetDetails(id);
    }

    public bool CreateOrder(PurchaseOrderCreateViewModel input, out string? errorMessage)
    {
        if (!dbContext.Suppliers.Any(supplier => supplier.Id == input.SupplierId))
        {
            errorMessage = "Le fournisseur selectionne est introuvable.";
            return false;
        }

        var validLines = (input.Lines ?? [])
            .Where(line => line.HasValue)
            .ToList();

        if (!validLines.Any())
        {
            errorMessage = "Ajoute au moins une ligne de commande.";
            return false;
        }

        if (dbContext.PurchaseOrders.Any(order => order.OrderNumber == input.OrderNumber))
        {
            errorMessage = "Ce numero de commande existe deja.";
            return false;
        }

        foreach (var line in validLines)
        {
            if (!line.StockItemId.HasValue || !line.QuantityOrdered.HasValue || !line.UnitPrice.HasValue)
            {
                errorMessage = "Chaque ligne renseignee doit contenir l'article, la quantite et le prix unitaire.";
                return false;
            }

            if (!dbContext.StockItems.Any(item => item.Id == line.StockItemId.Value))
            {
                errorMessage = "Une ligne de commande contient un article introuvable.";
                return false;
            }
        }

        var order = new PurchaseOrder
        {
            OrderNumber = input.OrderNumber,
            SupplierId = input.SupplierId,
            OrderDate = input.OrderDate,
            ExpectedDeliveryDate = input.ExpectedDeliveryDate,
            Notes = input.Notes,
            Status = PurchaseOrderStatus.Commandee,
            Lines = validLines
                .Select(line => new PurchaseOrderLine
                {
                    StockItemId = line.StockItemId!.Value,
                    QuantityOrdered = line.QuantityOrdered!.Value,
                    UnitPrice = line.UnitPrice!.Value
                })
                .ToList()
        };

        dbContext.PurchaseOrders.Add(order);
        dbContext.SaveChanges();

        errorMessage = null;
        return true;
    }

    public bool ReceiveOrder(int id, out string? errorMessage)
    {
        var order = dbContext.PurchaseOrders
            .Include(purchaseOrder => purchaseOrder.Lines)
            .ThenInclude(line => line.StockItem)
            .FirstOrDefault(purchaseOrder => purchaseOrder.Id == id);

        if (order is null)
        {
            errorMessage = "Commande introuvable.";
            return false;
        }

        if (order.Status == PurchaseOrderStatus.Recue)
        {
            errorMessage = "Cette commande a deja ete receptionnee.";
            return false;
        }

        if (order.Status == PurchaseOrderStatus.Annulee)
        {
            errorMessage = "Une commande annulee ne peut pas etre receptionnee.";
            return false;
        }

        foreach (var line in order.Lines)
        {
            if (line.StockItem is null)
            {
                errorMessage = "Une ligne de commande ne reference pas d'article valide.";
                return false;
            }

            line.StockItem.CurrentQuantity += line.QuantityOrdered;

            dbContext.StockMovements.Add(new StockMovement
            {
                StockItemId = line.StockItemId,
                MovementType = MovementType.Entree,
                Quantity = line.QuantityOrdered,
                Date = DateTime.Today,
                Notes = $"Reception commande {order.OrderNumber}"
            });
        }

        order.Status = PurchaseOrderStatus.Recue;
        dbContext.SaveChanges();

        errorMessage = null;
        return true;
    }

    private IReadOnlyList<SelectListItem> GetSupplierSelectList()
    {
        return dbContext.Suppliers
            .AsNoTracking()
            .OrderBy(supplier => supplier.Name)
            .Select(supplier => new SelectListItem
            {
                Value = supplier.Id.ToString(),
                Text = supplier.Name
            })
            .ToList();
    }

    private IReadOnlyList<SelectListItem> GetStockItemSelectList()
    {
        return dbContext.StockItems
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .Select(item => new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = $"{item.Name} ({item.Reference})"
            })
            .ToList();
    }

    private static List<PurchaseOrderLineInputModel> CreateDefaultLines()
    {
        return
        [
            new PurchaseOrderLineInputModel(),
            new PurchaseOrderLineInputModel(),
            new PurchaseOrderLineInputModel(),
            new PurchaseOrderLineInputModel(),
            new PurchaseOrderLineInputModel()
        ];
    }
}
