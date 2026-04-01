using MedicalStockManager.Models;
using MedicalStockManager.Services;

namespace MedicalStockManager.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext context, IConfiguration configuration)
    {
        if (!context.AppUsers.Any())
        {
            var authService = new AuthService(context);
            
            var adminPassword = configuration["SeedPasswords:AdminPassword"] ?? "Admin123!";
            var stockPassword = configuration["SeedPasswords:StockPassword"] ?? "Stock123!";
            var lecturePassword = configuration["SeedPasswords:LecturePassword"] ?? "Lecture123!";

            context.AppUsers.AddRange(
                new AppUser
                {
                    Username = "admin",
                    FullName = "Administrateur General",
                    Email = "admin@centre-diagnostic.local",
                    PasswordHash = authService.HashPassword(adminPassword),
                    Role = AppRole.Administrateur,
                    IsActive = true
                },
                new AppUser
                {
                    Username = "stock",
                    FullName = "Gestionnaire Stock",
                    Email = "stock@centre-diagnostic.local",
                    PasswordHash = authService.HashPassword(stockPassword),
                    Role = AppRole.GestionnaireStock,
                    IsActive = true
                },
                new AppUser
                {
                    Username = "lecture",
                    FullName = "Utilisateur Lecture",
                    Email = "lecture@centre-diagnostic.local",
                    PasswordHash = authService.HashPassword(lecturePassword),
                    Role = AppRole.Lecture,
                    IsActive = true
                });
            context.SaveChanges();
        }

        if (!context.Services.Any())
        {
            context.Services.AddRange(
                new Service { Name = "Imagerie" },
                new Service { Name = "Laboratoire Analyses" },
                new Service { Name = "Consultations" },
                new Service { Name = "Hopital du Jour" }
            );
            context.SaveChanges();
        }

        if (!context.Suppliers.Any())
        {
            context.Suppliers.AddRange(
                new Supplier
                {
                    Name = "MedSupply Afrique",
                    ContactName = "Aicha Traore",
                    Phone = "+234 800 111 2233",
                    Email = "contact@medsupply-afrique.example"
                },
                new Supplier
                {
                    Name = "BioLab Distribution",
                    ContactName = "Koffi Mensah",
                    Phone = "+233 30 222 9988",
                    Email = "ventes@biolab-distribution.example"
                });
            context.SaveChanges();
        }

        var imagerie = context.Services.First(s => s.Name == "Imagerie");
        var labo = context.Services.First(s => s.Name == "Laboratoire Analyses");
        var consult = context.Services.First(s => s.Name == "Consultations");
        var hdj = context.Services.First(s => s.Name == "Hopital du Jour");

        if (!context.Locations.Any())
        {
            context.Locations.Add(new Location
            {
                Name = "Pharmacie Centrale",
                IsCentral = true
            });
            context.SaveChanges();
        }

        var centralLocation = context.Locations.First(l => l.IsCentral);

        if (!context.StockItems.Any())
        {
            var items = new List<StockItem>
            {
                new StockItem { Name = "Films radiologiques", Reference = "IMG-001", ServiceId = imagerie.Id, CurrentQuantity = 14, AlertThreshold = 10, Unit = "boites", ExpirationDate = DateTime.Today.AddMonths(8) },
                new StockItem { Name = "Tubes EDTA", Reference = "LAB-014", ServiceId = labo.Id, CurrentQuantity = 9, AlertThreshold = 15, Unit = "paquets", ExpirationDate = DateTime.Today.AddMonths(4) },
                new StockItem { Name = "Gants d'examen", Reference = "CON-003", ServiceId = consult.Id, CurrentQuantity = 22, AlertThreshold = 20, Unit = "cartons", ExpirationDate = DateTime.Today.AddMonths(10) },
                new StockItem { Name = "Perfuseurs", Reference = "HDJ-022", ServiceId = hdj.Id, CurrentQuantity = 5, AlertThreshold = 12, Unit = "boites", ExpirationDate = DateTime.Today.AddMonths(2) }
            };
            
            context.StockItems.AddRange(items);
            context.SaveChanges();
            
            // Generate associated batches for the seeded items
            var batches = items.Select(i => new StockBatch
            {
                StockItemId = i.Id,
                LocationId = centralLocation.Id,
                BatchNumber = "LOT-INITIAL-" + i.Reference,
                ExpirationDate = i.ExpirationDate ?? DateTime.Today.AddYears(1),
                Quantity = i.CurrentQuantity
            }).ToList();
            
            context.StockBatches.AddRange(batches);
            context.SaveChanges();
        }
    }
}
