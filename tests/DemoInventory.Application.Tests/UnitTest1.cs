using DemoInventory.Application.DTOs;
using DemoInventory.Application.Services;
using DemoInventory.Domain.Entities;
using DemoInventory.Domain.Interfaces;
using Moq;

namespace DemoInventory.Application.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _service = new ProductService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_Should_Return_ProductDto_When_Product_Exists()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            SKU = "TEST-001",
            Price = 10.99m,
            QuantityInStock = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
                      .ReturnsAsync(product);

        // Act
        var result = await _service.GetProductByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal("TEST-001", result.SKU);
        Assert.Equal(10.99m, result.Price);
        Assert.Equal(100, result.QuantityInStock);
    }

    [Fact]
    public async Task GetProductByIdAsync_Should_Return_Null_When_Product_Does_Not_Exist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(1))
                      .ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetProductByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateProductAsync_Should_Create_Product_And_Return_Dto()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "New Description",
            SKU = "NEW-001",
            Price = 15.99m,
            QuantityInStock = 50
        };

        var createdProduct = new Product
        {
            Id = 1,
            Name = createDto.Name,
            Description = createDto.Description,
            SKU = createDto.SKU,
            Price = createDto.Price,
            QuantityInStock = createDto.QuantityInStock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
                      .ReturnsAsync(createdProduct);

        // Act
        var result = await _service.CreateProductAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("New Product", result.Name);
        Assert.Equal("New Description", result.Description);
        Assert.Equal("NEW-001", result.SKU);
        Assert.Equal(15.99m, result.Price);
        Assert.Equal(50, result.QuantityInStock);
    }

    [Fact]
    public async Task GetProductsByPriceRangeAsync_Should_Return_Products_In_Range()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Cheap Product",
                Description = "Low price",
                SKU = "CHEAP-001",
                Price = 5.99m,
                QuantityInStock = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Mid Product",
                Description = "Mid price",
                SKU = "MID-001",
                Price = 15.99m,
                QuantityInStock = 20,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var priceRange = new PriceRangeDto
        {
            MinPrice = 5.00m,
            MaxPrice = 20.00m
        };

        _mockRepository.Setup(r => r.GetByPriceRangeAsync(5.00m, 20.00m))
                      .ReturnsAsync(products);

        // Act
        var result = await _service.GetProductsByPriceRangeAsync(priceRange);

        // Assert
        Assert.NotNull(result);
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("Cheap Product", resultList[0].Name);
        Assert.Equal("Mid Product", resultList[1].Name);
    }
}