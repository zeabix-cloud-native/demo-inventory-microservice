using DemoInventory.Application.DTOs;
using DemoInventory.Infrastructure.Services;

namespace DemoInventory.Infrastructure.Tests;

public class InMemoryProductServiceTests
{
    private readonly InMemoryProductService _service;

    public InMemoryProductServiceTests()
    {
        _service = new InMemoryProductService();
    }

    [Fact]
    public async Task GetProductByIdAsync_Should_Return_Null_When_Product_Does_Not_Exist()
    {
        // Act
        var result = await _service.GetProductByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateProductAsync_Should_Create_Product_And_Return_Dto()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            SKU = "TEST-001",
            Price = 19.99m,
            QuantityInStock = 100
        };

        // Act
        var result = await _service.CreateProductAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal("TEST-001", result.SKU);
        Assert.Equal(19.99m, result.Price);
        Assert.Equal(100, result.QuantityInStock);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
        Assert.True(result.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task GetProductByIdAsync_Should_Return_ProductDto_When_Product_Exists()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            SKU = "TEST-001",
            Price = 19.99m,
            QuantityInStock = 100
        };
        var createdProduct = await _service.CreateProductAsync(createDto);

        // Act
        var result = await _service.GetProductByIdAsync(createdProduct.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdProduct.Id, result.Id);
        Assert.Equal("Test Product", result.Name);
    }

    [Fact]
    public async Task GetAllProductsAsync_Should_Return_All_Products()
    {
        // Arrange
        await _service.CreateProductAsync(new CreateProductDto
        {
            Name = "Product 1",
            Description = "Description 1",
            SKU = "PROD-001",
            Price = 10.99m,
            QuantityInStock = 50
        });

        await _service.CreateProductAsync(new CreateProductDto
        {
            Name = "Product 2",
            Description = "Description 2",
            SKU = "PROD-002",
            Price = 15.99m,
            QuantityInStock = 30
        });

        // Act
        var result = await _service.GetAllProductsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateProductAsync_Should_Update_Product_Successfully()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Original Product",
            Description = "Original Description",
            SKU = "ORIG-001",
            Price = 10.00m,
            QuantityInStock = 100
        };
        var createdProduct = await _service.CreateProductAsync(createDto);

        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 15.00m,
            QuantityInStock = 200
        };

        // Act
        var result = await _service.UpdateProductAsync(createdProduct.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdProduct.Id, result.Id);
        Assert.Equal("Updated Product", result.Name);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(15.00m, result.Price);
        Assert.Equal(200, result.QuantityInStock);
        Assert.True(result.UpdatedAt > result.CreatedAt);
    }

    [Fact]
    public async Task UpdateProductAsync_Should_Throw_Exception_When_Product_Not_Found()
    {
        // Arrange
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 15.00m,
            QuantityInStock = 200
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateProductAsync(999, updateDto));
    }

    [Fact]
    public async Task DeleteProductAsync_Should_Remove_Product()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Product to Delete",
            Description = "Description",
            SKU = "DEL-001",
            Price = 10.00m,
            QuantityInStock = 50
        };
        var createdProduct = await _service.CreateProductAsync(createDto);

        // Act
        await _service.DeleteProductAsync(createdProduct.Id);

        // Assert
        var result = await _service.GetProductByIdAsync(createdProduct.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProductBySkuAsync_Should_Return_Product_When_Exists()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "SKU Test Product",
            Description = "Description",
            SKU = "SKU-TEST-001",
            Price = 25.00m,
            QuantityInStock = 75
        };
        await _service.CreateProductAsync(createDto);

        // Act
        var result = await _service.GetProductBySkuAsync("SKU-TEST-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SKU Test Product", result.Name);
        Assert.Equal("SKU-TEST-001", result.SKU);
    }

    [Fact]
    public async Task GetProductBySkuAsync_Should_Return_Null_When_Not_Exists()
    {
        // Act
        var result = await _service.GetProductBySkuAsync("NON-EXISTENT");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchProductsAsync_Should_Return_Matching_Products()
    {
        // Arrange
        await _service.CreateProductAsync(new CreateProductDto
        {
            Name = "Apple iPhone",
            Description = "Smartphone",
            SKU = "APPLE-001",
            Price = 999.99m,
            QuantityInStock = 10
        });

        await _service.CreateProductAsync(new CreateProductDto
        {
            Name = "Apple iPad",
            Description = "Tablet",
            SKU = "APPLE-002",
            Price = 599.99m,
            QuantityInStock = 15
        });

        await _service.CreateProductAsync(new CreateProductDto
        {
            Name = "Samsung Phone",
            Description = "Android Phone",
            SKU = "SAMSUNG-001",
            Price = 799.99m,
            QuantityInStock = 20
        });

        // Act
        var result = await _service.SearchProductsAsync("Apple");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.Contains("Apple", p.Name));
    }

    [Fact]
    public async Task SearchProductsAsync_Should_Be_Case_Insensitive()
    {
        // Arrange
        await _service.CreateProductAsync(new CreateProductDto
        {
            Name = "Test Product",
            Description = "Description",
            SKU = "TEST-001",
            Price = 10.00m,
            QuantityInStock = 50
        });

        // Act
        var result = await _service.SearchProductsAsync("test");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Product", result.First().Name);
    }
}