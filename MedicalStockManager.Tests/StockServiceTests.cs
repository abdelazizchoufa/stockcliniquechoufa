using MedicalStockManager.Data;
using MedicalStockManager.Models;
using MedicalStockManager.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalStockManager.Tests;

public class StockServiceTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void AddItem_NewUniqueItem_ReturnsTrue()
    {
        // Arrange
        var context = GetInMemoryContext();
        var stockService = new StockService(context);
        
        // Add a service
        var service = new Service { Name = "Imagerie" };
        context.Services.Add(service);
        context.SaveChanges();

        var newItem = new StockItemFormViewModel
        {
            Name = "Gants",
            Reference = "GNT-001",
            ServiceId = service.Id,
            CurrentQuantity = 100,
            AlertThreshold = 20,
            Unit = "boite"
        };

        // Act
        var success = stockService.AddItem(newItem, out var error);

        // Assert
        Assert.True(success);
        Assert.Null(error);
        
        var itemInDb = context.StockItems.FirstOrDefault(i => i.Reference == "GNT-001");
        Assert.NotNull(itemInDb);
        Assert.Equal("Gants", itemInDb.Name);
    }

    [Fact]
    public void AddItem_DuplicateReference_ReturnsFalse()
    {
        // Arrange
        var context = GetInMemoryContext();
        var stockService = new StockService(context);
        
        var service = new Service { Name = "Imagerie" };
        context.Services.Add(service);
        
        var existingItem = new StockItem
        {
            Name = "Existing Gants",
            Reference = "GNT-001",
            ServiceId = service.Id,
            Unit = "boite"
        };
        context.StockItems.Add(existingItem);
        context.SaveChanges();

        var newItem = new StockItemFormViewModel
        {
            Name = "New Gants",
            Reference = "GNT-001", // Duplicate
            ServiceId = service.Id,
            Unit = "boite"
        };

        // Act
        var success = stockService.AddItem(newItem, out var error);

        // Assert
        Assert.False(success);
        Assert.NotNull(error);
        Assert.Contains("existe", error.ToLower());
    }
}
