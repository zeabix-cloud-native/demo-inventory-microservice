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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Product_Name_Should_Throw_When_NullOrEmpty(string invalidName)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Product { Name = invalidName });
        Assert.Contains("Product name cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Product_Name_Should_Throw_When_TooLong()
    {
        // Arrange
        var longName = new string('A', 201);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Product { Name = longName });
        Assert.Contains("Product name cannot exceed 200 characters", exception.Message);
    }

    [Fact]
    public void Product_Name_Should_Trim_Whitespace()
    {
        // Arrange & Act
        var product = new Product { Name = "  Test Product  " };
        
        // Assert
        Assert.Equal("Test Product", product.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Product_SKU_Should_Throw_When_NullOrEmpty(string invalidSku)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Product { SKU = invalidSku });
        Assert.Contains("SKU cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Product_SKU_Should_Throw_When_TooLong()
    {
        // Arrange
        var longSku = new string('A', 51);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Product { SKU = longSku });
        Assert.Contains("SKU cannot exceed 50 characters", exception.Message);
    }

    [Theory]
    [InlineData("TEST@001")]
    [InlineData("TEST 001")]
    [InlineData("TEST.001")]
    public void Product_SKU_Should_Throw_When_InvalidFormat(string invalidSku)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Product { SKU = invalidSku });
        Assert.Contains("SKU must contain only uppercase letters, numbers, and hyphens", exception.Message);
    }

    [Fact]
    public void Product_SKU_Should_ConvertToUppercase()
    {
        // Arrange & Act - lowercase input should be converted to uppercase
        var product = new Product { SKU = "test-001" };
        
        // Assert - should be converted to uppercase
        Assert.Equal("TEST-001", product.SKU);
        
        // Test that it trims and converts to uppercase
        product.SKU = " another-sku ";
        Assert.Equal("ANOTHER-SKU", product.SKU);
    }

    [Fact]
    public void Product_Price_Should_Throw_When_Negative()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Product { Price = -1.0m });
        Assert.Contains("Price cannot be negative", exception.Message);
    }

    [Fact]
    public void Product_Price_Should_Throw_When_TooHigh()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Product { Price = 1000000.0m });
        Assert.Contains("Price cannot exceed 999,999.99", exception.Message);
    }

    [Fact]
    public void Product_Price_Should_Accept_Zero()
    {
        // Arrange & Act
        var product = new Product { Price = 0.0m };
        
        // Assert
        Assert.Equal(0.0m, product.Price);
    }

    [Fact]
    public void Product_QuantityInStock_Should_Throw_When_Negative()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Product { QuantityInStock = -1 });
        Assert.Contains("Quantity in stock cannot be negative", exception.Message);
    }

    [Fact]
    public void Product_QuantityInStock_Should_Accept_Zero()
    {
        // Arrange & Act
        var product = new Product { QuantityInStock = 0 };
        
        // Assert
        Assert.Equal(0, product.QuantityInStock);
    }

    [Fact]
    public void Product_Description_Should_Throw_When_TooLong()
    {
        // Arrange
        var product = new Product();
        var longDescription = new string('A', 1001);
        product.Description = longDescription;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => product.ValidateDescription());
        Assert.Contains("Description cannot exceed 1000 characters", exception.Message);
    }

    [Fact]
    public void Product_Validate_Should_Pass_With_Valid_Product()
    {
        // Arrange
        var product = new Product
        {
            Name = "Valid Product",
            SKU = "VALID-001",
            Price = 19.99m,
            QuantityInStock = 50,
            Description = "Valid description"
        };
        
        // Act & Assert - Should not throw
        product.Validate();
    }
}