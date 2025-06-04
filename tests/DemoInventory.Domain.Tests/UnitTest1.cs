using DemoInventory.Domain.Entities;

namespace DemoInventory.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void Product_Should_Create_With_Valid_Properties()
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

        // Assert
        Assert.Equal(1, product.Id);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("Test Description", product.Description);
        Assert.Equal("TEST-001", product.SKU);
        Assert.Equal(10.99m, product.Price);
        Assert.Equal(100, product.QuantityInStock);
        Assert.True(product.CreatedAt <= DateTime.UtcNow);
        Assert.True(product.UpdatedAt <= DateTime.UtcNow);
    }
}