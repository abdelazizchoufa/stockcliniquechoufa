using MedicalStockManager.Models;
using MedicalStockManager.Services;

namespace MedicalStockManager.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext context)
    {
        if (!context.AppUsers.Any())
        {
            var authService = new AuthService(context);

            context.AppUsers.AddRange(
                new AppUser
                {
                    Username = "admin",
                    FullName = "Administrateur General",
                    Email = "admin@centre-diagnostic.local",
                    PasswordHash = authService.HashPassword("Admin123!"),
                    Role = AppRole.Administrateur,
                    IsActive = true
                },
                new AppUser
                {
                    Username = "stock",
                    FullName = "Gestionnaire Stock",
                    Email = "stock@centre-diagnostic.local",
                    PasswordHash = authService.HashPassword("Stock123!"),
                    Role = AppRole.GestionnaireStock,
                    IsActive = true
                },
                new AppUser
                {
                    Username = "lecture",
                    FullName = "Utilisateur Lecture",
                    Email = "lecture@centre-diagnostic.local",
                    PasswordHash = authService.HashPassword("Lecture123!"),
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

        if (context.StockItems.Any()) return;

        var imagerie = context.Services.First(s => s.Name == "Imagerie");
        var labo = context.Services.First(s => s.Name == "Laboratoire Analyses");
        var consult = context.Services.First(s => s.Name == "Consultations");
        var hdj = context.Services.First(s => s.Name == "Hopital du Jour");

        context.StockItems.AddRange(
            new StockItem
            {
                Name = "Films radiologiques",
                Reference = "IMG-001",
                ServiceId = imagerie.Id,
                CurrentQuantity = 14,
                AlertThreshold = 10,
                Unit = "boites",
                ExpirationDate = DateTime.Today.AddMonths(8)
            },
            new StockItem
            {
                Name = "Tubes EDTA",
                Reference = "LAB-014",
                ServiceId = labo.Id,
                CurrentQuantity = 9,
                AlertThreshold = 15,
                Unit = "paquets",
                ExpirationDate = DateTime.Today.AddMonths(4)
            },
            new StockItem
            {
                Name = "Gants d'examen",
                Reference = "CON-003",
                ServiceId = consult.Id,
                CurrentQuantity = 22,
                AlertThreshold = 20,
                Unit = "cartons",
                ExpirationDate = DateTime.Today.AddMonths(10)
            },
            new StockItem
            {
                Name = "Perfuseurs",
                Reference = "HDJ-022",
                ServiceId = hdj.Id,
                CurrentQuantity = 5,
                AlertThreshold = 12,
                Unit = "boites",
                ExpirationDate = DateTime.Today.AddMonths(2)
            }
        );
        context.SaveChanges();
    }
}
