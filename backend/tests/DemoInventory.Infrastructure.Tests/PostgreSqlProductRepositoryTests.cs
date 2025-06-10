using DemoInventory.Domain.Entities;
using DemoInventory.Infrastructure.Data;
using DemoInventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DemoInventory.Infrastructure.Tests;

public class PostgreSqlProductRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PostgreSqlProductRepository _repository;

    public PostgreSqlProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new PostgreSqlProductRepository(_context);
    }

    [Fact]
    public async Task AddAsync_Should_Add_Product_And_Return_With_Id()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            SKU = "TEST-001",
            Price = 10.99m,
            QuantityInStock = 100
        };

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal("TEST-001", result.SKU);
        Assert.True(result.CreatedAt > DateTime.MinValue);
        Assert.True(result.UpdatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Product_When_Exists()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            SKU = "TEST-002",
            Price = 15.99m,
            QuantityInStock = 50
        };
        var addedProduct = await _repository.AddAsync(product);

        // Act
        var result = await _repository.GetByIdAsync(addedProduct.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedProduct.Id, result.Id);
        Assert.Equal("Test Product", result.Name);
    }

    [Fact]
    public async Task GetBySkuAsync_Should_Return_Product_When_Exists()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            SKU = "TEST-003",
            Price = 20.99m,
            QuantityInStock = 25
        };
        await _repository.AddAsync(product);

        // Act
        var result = await _repository.GetBySkuAsync("TEST-003");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEST-003", result.SKU);
        Assert.Equal("Test Product", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Product_Successfully()
    {
        // Arrange
        var product = new Product
        {
            Name = "Original Product",
            Description = "Original Description",
            SKU = "TEST-004",
            Price = 25.99m,
            QuantityInStock = 75
        };
        var addedProduct = await _repository.AddAsync(product);

        // Update the product
        addedProduct.Name = "Updated Product";
        addedProduct.Price = 30.99m;

        // Act
        var result = await _repository.UpdateAsync(addedProduct);

        // Assert
        Assert.Equal("Updated Product", result.Name);
        Assert.Equal(30.99m, result.Price);
        Assert.True(result.UpdatedAt > result.CreatedAt);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Product()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            SKU = "TEST-005",
            Price = 35.99m,
            QuantityInStock = 10
        };
        var addedProduct = await _repository.AddAsync(product);

        // Act
        await _repository.DeleteAsync(addedProduct.Id);

        // Assert
        var result = await _repository.GetByIdAsync(addedProduct.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchByNameAsync_Should_Return_Matching_Products()
    {
        // Arrange
        await _repository.AddAsync(new Product
        {
            Name = "Apple iPhone",
            Description = "Smartphone",
            SKU = "APPLE-001",
            Price = 999.99m,
            QuantityInStock = 5
        });
        
        await _repository.AddAsync(new Product
        {
            Name = "Apple iPad",
            Description = "Tablet",
            SKU = "APPLE-002",
            Price = 599.99m,
            QuantityInStock = 10
        });

        await _repository.AddAsync(new Product
        {
            Name = "Samsung Galaxy",
            Description = "Smartphone",
            SKU = "SAMSUNG-001",
            Price = 899.99m,
            QuantityInStock = 8
        });

        // Act
        var results = await _repository.SearchByNameAsync("Apple");

        // Assert
        Assert.Equal(2, results.Count());
        Assert.All(results, p => Assert.Contains("Apple", p.Name));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}