using DemoInventory.API.Controllers;
using DemoInventory.Application.DTOs;
using DemoInventory.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DemoInventory.API.Tests;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockProductService = new Mock<IProductService>();
        _controller = new ProductsController(_mockProductService.Object);
    }

    [Fact]
    public async Task GetProducts_Should_Return_Ok_With_Products()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new ProductDto { Id = 1, Name = "Product 1", SKU = "SKU-001", Price = 10.99m, QuantityInStock = 100 },
            new ProductDto { Id = 2, Name = "Product 2", SKU = "SKU-002", Price = 20.99m, QuantityInStock = 50 }
        };

        _mockProductService.Setup(s => s.GetAllProductsAsync())
                          .ReturnsAsync(products);

        // Act
        var result = await _controller.GetProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
    }

    [Fact]
    public async Task GetProduct_Should_Return_Ok_When_Product_Exists()
    {
        // Arrange
        var productId = 1;
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            SKU = "TEST-001",
            Price = 15.99m,
            QuantityInStock = 75
        };

        _mockProductService.Setup(s => s.GetProductByIdAsync(productId))
                          .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<ProductDto>(okResult.Value);
        Assert.Equal(productId, returnedProduct.Id);
        Assert.Equal("Test Product", returnedProduct.Name);
    }

    [Fact]
    public async Task GetProduct_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        var productId = 999;
        _mockProductService.Setup(s => s.GetProductByIdAsync(productId))
                          .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetProductBySku_Should_Return_Ok_When_Product_Exists()
    {
        // Arrange
        var sku = "TEST-001";
        var product = new ProductDto
        {
            Id = 1,
            Name = "Test Product",
            SKU = sku,
            Price = 15.99m,
            QuantityInStock = 75
        };

        _mockProductService.Setup(s => s.GetProductBySkuAsync(sku))
                          .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProductBySku(sku);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<ProductDto>(okResult.Value);
        Assert.Equal(sku, returnedProduct.SKU);
    }

    [Fact]
    public async Task GetProductBySku_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        var sku = "NONEXISTENT";
        _mockProductService.Setup(s => s.GetProductBySkuAsync(sku))
                          .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.GetProductBySku(sku);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task SearchProducts_Should_Return_Ok_With_Matching_Products()
    {
        // Arrange
        var searchTerm = "test";
        var products = new List<ProductDto>
        {
            new ProductDto { Id = 1, Name = "Test Product 1", SKU = "TEST-001", Price = 10.99m, QuantityInStock = 100 }
        };

        _mockProductService.Setup(s => s.SearchProductsAsync(searchTerm))
                          .ReturnsAsync(products);

        // Act
        var result = await _controller.SearchProducts(searchTerm);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.Single(returnedProducts);
    }

    [Fact]
    public async Task CreateProduct_Should_Return_CreatedAtAction_With_Product()
    {
        // Arrange
        var createProductDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "New Description",
            SKU = "NEW-001",
            Price = 25.99m,
            QuantityInStock = 30
        };

        var createdProduct = new ProductDto
        {
            Id = 1,
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            SKU = createProductDto.SKU,
            Price = createProductDto.Price,
            QuantityInStock = createProductDto.QuantityInStock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockProductService.Setup(s => s.CreateProductAsync(createProductDto))
                          .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.CreateProduct(createProductDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedProduct = Assert.IsType<ProductDto>(createdAtActionResult.Value);
        Assert.Equal(createProductDto.Name, returnedProduct.Name);
        Assert.Equal(createProductDto.SKU, returnedProduct.SKU);
        Assert.Equal("GetProduct", createdAtActionResult.ActionName);
    }

    [Fact]
    public async Task UpdateProduct_Should_Return_Ok_When_Product_Exists()
    {
        // Arrange
        var productId = 1;
        var updateProductDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 35.99m,
            QuantityInStock = 40
        };

        var updatedProduct = new ProductDto
        {
            Id = productId,
            Name = updateProductDto.Name,
            Description = updateProductDto.Description,
            Price = updateProductDto.Price,
            QuantityInStock = updateProductDto.QuantityInStock,
            UpdatedAt = DateTime.UtcNow
        };

        _mockProductService.Setup(s => s.UpdateProductAsync(productId, updateProductDto))
                          .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.UpdateProduct(productId, updateProductDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProduct = Assert.IsType<ProductDto>(okResult.Value);
        Assert.Equal(updateProductDto.Name, returnedProduct.Name);
        Assert.Equal(productId, returnedProduct.Id);
    }

    [Fact]
    public async Task UpdateProduct_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        var productId = 999;
        var updateProductDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 35.99m,
            QuantityInStock = 40
        };

        _mockProductService.Setup(s => s.UpdateProductAsync(productId, updateProductDto))
                          .ThrowsAsync(new InvalidOperationException($"Product with ID {productId} not found"));

        // Act
        var result = await _controller.UpdateProduct(productId, updateProductDto);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteProduct_Should_Return_NoContent()
    {
        // Arrange
        var productId = 1;
        _mockProductService.Setup(s => s.DeleteProductAsync(productId))
                          .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockProductService.Verify(s => s.DeleteProductAsync(productId), Times.Once);
    }
}