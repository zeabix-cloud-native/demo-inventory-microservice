using DemoInventory.Application.DTOs;
using DemoInventory.Application.Interfaces;
using DemoInventory.Domain.Entities;

namespace DemoInventory.Infrastructure.Services;

/// <summary>
/// In-memory implementation of the product service for testing and development purposes
/// </summary>
public class InMemoryProductService : IProductService
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;

    /// <summary>
    /// Retrieves a product by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the product</param>
    /// <returns>The product DTO if found, otherwise null</returns>
    public Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product == null ? null : MapToDto(product));
    }

    /// <summary>
    /// Retrieves all products from the in-memory collection
    /// </summary>
    /// <returns>A collection of all product DTOs</returns>
    public Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var productDtos = _products.Select(MapToDto);
        return Task.FromResult(productDtos);
    }

    /// <summary>
    /// Creates a new product in the in-memory collection
    /// </summary>
    /// <param name="createProductDto">The product creation data</param>
    /// <returns>The created product DTO</returns>
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

    /// <summary>
    /// Updates an existing product in the in-memory collection
    /// </summary>
    /// <param name="id">The unique identifier of the product to update</param>
    /// <param name="updateProductDto">The product update data</param>
    /// <returns>The updated product DTO</returns>
    /// <exception cref="InvalidOperationException">Thrown when the product is not found</exception>
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

    /// <summary>
    /// Deletes a product from the in-memory collection by its ID
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task DeleteProductAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a product by its Stock Keeping Unit (SKU)
    /// </summary>
    /// <param name="sku">The SKU of the product to retrieve</param>
    /// <returns>The product DTO if found, otherwise null</returns>
    public Task<ProductDto?> GetProductBySkuAsync(string sku)
    {
        var product = _products.FirstOrDefault(p => p.SKU == sku);
        return Task.FromResult(product == null ? null : MapToDto(product));
    }

    /// <summary>
    /// Retrieves a product by its exact name (case-insensitive)
    /// </summary>
    /// <param name="name">The name of the product to retrieve</param>
    /// <returns>The product DTO if found, otherwise null</returns>
    public Task<ProductDto?> GetProductByNameAsync(string name)
    {
        var product = _products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(product == null ? null : MapToDto(product));
    }

    /// <summary>
    /// Searches for products by name using partial matching (case-insensitive)
    /// </summary>
    /// <param name="searchTerm">The search term to match against product names</param>
    /// <returns>A collection of product DTOs whose names contain the search term</returns>
    public Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        var products = _products.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        var productDtos = products.Select(MapToDto);
        return Task.FromResult(productDtos);
    }

    /// <summary>
    /// Retrieves products within a specified price range
    /// </summary>
    /// <param name="priceRange">The price range criteria</param>
    /// <returns>A collection of product DTOs within the specified price range</returns>
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