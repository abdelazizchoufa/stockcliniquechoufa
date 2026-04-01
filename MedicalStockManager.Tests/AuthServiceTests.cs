using MedicalStockManager.Data;
using MedicalStockManager.Models;
using MedicalStockManager.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalStockManager.Tests;

public class AuthServiceTests
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
    public async Task ValidateUserAsync_ValidCredentials_ReturnsUser()
    {
        // Arrange
        var context = GetInMemoryContext();
        var authService = new AuthService(context);
        
        var user = new AppUser
        {
            Username = "testuser",
            FullName = "Test User",
            Email = "test@example.com",
            PasswordHash = authService.HashPassword("Password123!"),
            Role = AppRole.Lecture,
            IsActive = true
        };
        
        context.AppUsers.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await authService.ValidateUserAsync("testuser", "Password123!");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task ValidateUserAsync_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var context = GetInMemoryContext();
        var authService = new AuthService(context);
        
        var user = new AppUser
        {
            Username = "testuser",
            FullName = "Test User",
            Email = "test@example.com",
            PasswordHash = authService.HashPassword("Password123!"),
            Role = AppRole.Lecture,
            IsActive = true
        };
        
        context.AppUsers.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await authService.ValidateUserAsync("testuser", "WrongPassword!");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateUserAsync_InactiveUser_ReturnsNull()
    {
        // Arrange
        var context = GetInMemoryContext();
        var authService = new AuthService(context);
        
        var user = new AppUser
        {
            Username = "testuser",
            FullName = "Test User",
            Email = "test@example.com",
            PasswordHash = authService.HashPassword("Password123!"),
            Role = AppRole.Lecture,
            IsActive = false // Inactive
        };
        
        context.AppUsers.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await authService.ValidateUserAsync("testuser", "Password123!");

        // Assert
        Assert.Null(result);
    }
}
