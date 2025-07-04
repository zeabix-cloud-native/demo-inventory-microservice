using DemoInventory.Application.DTOs;
using DemoInventory.Application.Interfaces;
using DemoInventory.Domain.Entities;

namespace DemoInventory.Infrastructure.Services;

public class InMemoryProductService : IProductService
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;

    public Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product == null ? null : MapToDto(product));
    }

    public Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var productDtos = _products.Select(MapToDto);
        return Task.FromResult(productDtos);
    }

    public Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        var product = new Product
        {
            Id = _nextId++,
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            SKU = createProductDto.SKU,
            Price = createProductDto.Price,
            QuantityInStock = createProductDto.QuantityInStock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _products.Add(product);
        return Task.FromResult(MapToDto(product));
    }

    public Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {id} not found");

        product.Name = updateProductDto.Name;
        product.Description = updateProductDto.Description;
        product.Price = updateProductDto.Price;
        product.QuantityInStock = updateProductDto.QuantityInStock;
        product.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult(MapToDto(product));
    }

    public Task DeleteProductAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
        }
        return Task.CompletedTask;
    }

    public Task<ProductDto?> GetProductBySkuAsync(string sku)
    {
        var product = _products.FirstOrDefault(p => p.SKU == sku);
        return Task.FromResult(product == null ? null : MapToDto(product));
    }

    public Task<ProductDto?> GetProductByNameAsync(string name)
    {
        var product = _products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(product == null ? null : MapToDto(product));
    }

    public Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        var products = _products.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        var productDtos = products.Select(MapToDto);
        return Task.FromResult(productDtos);
    }

    public Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(PriceRangeDto priceRange)
    {
        var products = _products.Where(p => p.Price >= priceRange.MinPrice && p.Price <= priceRange.MaxPrice);
        var productDtos = products.Select(MapToDto);
        return Task.FromResult(productDtos);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Price = product.Price,
            QuantityInStock = product.QuantityInStock,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}