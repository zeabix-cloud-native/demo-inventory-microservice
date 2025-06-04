using System.Text.Json;
using DemoInventory.Application.DTOs;

namespace DemoInventory.Application.Tests;

public class AxiosIntegrationTests
{
    [Fact]
    public void ProductDto_Should_Serialize_To_Json_Correctly()
    {
        // Arrange
        var productDto = new ProductDto
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            SKU = "TEST-001",
            Price = 99.99m,
            QuantityInStock = 10,
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(productDto, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        // Assert
        Assert.Contains("\"id\":1", json);
        Assert.Contains("\"name\":\"Test Product\"", json);
        Assert.Contains("\"description\":\"Test Description\"", json);
        Assert.Contains("\"sku\":\"TEST-001\"", json);
        Assert.Contains("\"price\":99.99", json);
        Assert.Contains("\"quantityInStock\":10", json);
    }

    [Fact]
    public void CreateProductDto_Should_Deserialize_From_Json_Correctly()
    {
        // Arrange
        var json = """
        {
            "name": "New Product",
            "description": "New Description",
            "sku": "NEW-001",
            "price": 49.99,
            "quantityInStock": 25
        }
        """;

        // Act
        var createDto = JsonSerializer.Deserialize<CreateProductDto>(json, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        // Assert
        Assert.NotNull(createDto);
        Assert.Equal("New Product", createDto.Name);
        Assert.Equal("New Description", createDto.Description);
        Assert.Equal("NEW-001", createDto.SKU);
        Assert.Equal(49.99m, createDto.Price);
        Assert.Equal(25, createDto.QuantityInStock);
    }

    [Fact]
    public void UpdateProductDto_Should_Deserialize_From_Json_Correctly()
    {
        // Arrange
        var json = """
        {
            "name": "Updated Product",
            "description": "Updated Description",
            "price": 79.99,
            "quantityInStock": 15
        }
        """;

        // Act
        var updateDto = JsonSerializer.Deserialize<UpdateProductDto>(json, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        // Assert
        Assert.NotNull(updateDto);
        Assert.Equal("Updated Product", updateDto.Name);
        Assert.Equal("Updated Description", updateDto.Description);
        Assert.Equal(79.99m, updateDto.Price);
        Assert.Equal(15, updateDto.QuantityInStock);
    }
}