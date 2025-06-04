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
    public async Task CreateProductAsync_Should_Call_Repository_With_Correct_Product_Object()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            SKU = "test-123",
            Price = 25.50m,
            QuantityInStock = 100
        };

        var capturedProduct = (Product?)null;
        var createdProduct = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description", 
            SKU = "TEST-123",
            Price = 25.50m,
            QuantityInStock = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
                      .Callback<Product>(p => capturedProduct = p)
                      .ReturnsAsync(createdProduct);

        // Act
        await _service.CreateProductAsync(createDto);

        // Assert
        Assert.NotNull(capturedProduct);
        Assert.Equal("Test Product", capturedProduct.Name);
        Assert.Equal("Test Description", capturedProduct.Description);
        Assert.Equal("TEST-123", capturedProduct.SKU); // Should be uppercase
        Assert.Equal(25.50m, capturedProduct.Price);
        Assert.Equal(100, capturedProduct.QuantityInStock);
        Assert.True(capturedProduct.CreatedAt <= DateTime.UtcNow);
        Assert.True(capturedProduct.UpdatedAt <= DateTime.UtcNow);
        // Allow for slight timing differences in DateTime values
        Assert.True(Math.Abs((capturedProduct.CreatedAt - capturedProduct.UpdatedAt).TotalMilliseconds) < 10);
    }

    [Fact]
    public async Task CreateProductAsync_Should_Transform_SKU_To_Uppercase()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Product",
            Description = "Description",
            SKU = "lowercase-sku-123",
            Price = 10.00m,
            QuantityInStock = 5
        };

        var capturedProduct = (Product?)null;
        var createdProduct = new Product
        {
            Id = 1,
            Name = "Product",
            Description = "Description",
            SKU = "LOWERCASE-SKU-123",
            Price = 10.00m,
            QuantityInStock = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
                      .Callback<Product>(p => capturedProduct = p)
                      .ReturnsAsync(createdProduct);

        // Act
        var result = await _service.CreateProductAsync(createDto);

        // Assert
        Assert.NotNull(capturedProduct);
        Assert.Equal("LOWERCASE-SKU-123", capturedProduct.SKU);
        Assert.Equal("LOWERCASE-SKU-123", result.SKU);
    }

    [Fact]
    public async Task CreateProductAsync_Should_Set_CreatedAt_And_UpdatedAt_To_Current_Time()
    {
        // Arrange
        var beforeTime = DateTime.UtcNow;
        var createDto = new CreateProductDto
        {
            Name = "Time Test Product",
            Description = "Time test",
            SKU = "TIME-001",
            Price = 5.00m,
            QuantityInStock = 1
        };

        var capturedProduct = (Product?)null;
        var createdProduct = new Product
        {
            Id = 1,
            Name = "Time Test Product",
            Description = "Time test",
            SKU = "TIME-001",
            Price = 5.00m,
            QuantityInStock = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
                      .Callback<Product>(p => capturedProduct = p)
                      .ReturnsAsync(createdProduct);

        // Act
        await _service.CreateProductAsync(createDto);
        var afterTime = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedProduct);
        Assert.True(capturedProduct.CreatedAt >= beforeTime);
        Assert.True(capturedProduct.CreatedAt <= afterTime);
        Assert.True(capturedProduct.UpdatedAt >= beforeTime);
        Assert.True(capturedProduct.UpdatedAt <= afterTime);
        // Allow for slight timing differences in DateTime values  
        Assert.True(Math.Abs((capturedProduct.CreatedAt - capturedProduct.UpdatedAt).TotalMilliseconds) < 10);
    }

    [Fact]
    public async Task CreateProductAsync_Should_Handle_Minimum_Valid_Values()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "A", // Minimum 1 character
            Description = "", // Can be empty
            SKU = "A-1", // Minimum 3 characters
            Price = 0.01m, // Minimum positive value
            QuantityInStock = 0 // Minimum non-negative
        };

        var createdProduct = new Product
        {
            Id = 1,
            Name = "A",
            Description = "",
            SKU = "A-1",
            Price = 0.01m,
            QuantityInStock = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
                      .ReturnsAsync(createdProduct);

        // Act
        var result = await _service.CreateProductAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("A", result.Name);
        Assert.Equal("", result.Description);
        Assert.Equal("A-1", result.SKU);
        Assert.Equal(0.01m, result.Price);
        Assert.Equal(0, result.QuantityInStock);
    }

    [Fact]
    public async Task CreateProductAsync_Should_Handle_Maximum_Valid_Values()
    {
        // Arrange
        var longName = new string('X', 200); // Maximum 200 characters
        var longDescription = new string('Y', 1000); // Maximum 1000 characters
        var longSku = new string('Z', 50); // Maximum 50 characters
        
        var createDto = new CreateProductDto
        {
            Name = longName,
            Description = longDescription,
            SKU = longSku,
            Price = 999999.99m, // Maximum valid price
            QuantityInStock = int.MaxValue // Maximum valid quantity
        };

        var createdProduct = new Product
        {
            Id = 1,
            Name = longName,
            Description = longDescription,
            SKU = longSku,
            Price = 999999.99m,
            QuantityInStock = int.MaxValue,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
                      .ReturnsAsync(createdProduct);

        // Act
        var result = await _service.CreateProductAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(longName, result.Name);
        Assert.Equal(longDescription, result.Description);
        Assert.Equal(longSku, result.SKU);
        Assert.Equal(999999.99m, result.Price);
        Assert.Equal(int.MaxValue, result.QuantityInStock);
    }

    [Fact]
    public async Task CreateProductAsync_Should_Map_All_Properties_Correctly()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "  Mapping Test Product  ", // Test trimming
            Description = "Detailed mapping test description",
            SKU = "map-test-001",
            Price = 42.75m,
            QuantityInStock = 99
        };

        var createdProduct = new Product
        {
            Id = 42,
            Name = "Mapping Test Product", // Should be trimmed
            Description = "Detailed mapping test description",
            SKU = "MAP-TEST-001", // Should be uppercase
            Price = 42.75m,
            QuantityInStock = 99,
            CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
                      .ReturnsAsync(createdProduct);

        // Act
        var result = await _service.CreateProductAsync(createDto);

        // Assert - Verify complete mapping
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("Mapping Test Product", result.Name);
        Assert.Equal("Detailed mapping test description", result.Description);
        Assert.Equal("MAP-TEST-001", result.SKU);
        Assert.Equal(42.75m, result.Price);
        Assert.Equal(99, result.QuantityInStock);
        Assert.Equal(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc), result.CreatedAt);
        Assert.Equal(new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc), result.UpdatedAt);
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