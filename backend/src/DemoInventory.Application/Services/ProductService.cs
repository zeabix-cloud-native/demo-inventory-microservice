using DemoInventory.Application.DTOs;
using DemoInventory.Application.Interfaces;
using DemoInventory.Domain.Entities;
using DemoInventory.Domain.Interfaces;

namespace DemoInventory.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product == null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        // Create the product object first to normalize the SKU
        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            SKU = createProductDto.SKU,
            Price = createProductDto.Price,
            QuantityInStock = createProductDto.QuantityInStock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Check if a product with the same SKU already exists
        var existingProduct = await _productRepository.GetBySkuAsync(product.SKU);
        if (existingProduct != null)
        {
            throw new ArgumentException($"SKU '{product.SKU}' already exists. Each product must have a unique SKU.", nameof(createProductDto.SKU));
        }

        var createdProduct = await _productRepository.AddAsync(product);
        return MapToDto(createdProduct);
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {id} not found");

        product.Name = updateProductDto.Name;
        product.Description = updateProductDto.Description;
        product.Price = updateProductDto.Price;
        product.QuantityInStock = updateProductDto.QuantityInStock;
        product.UpdatedAt = DateTime.UtcNow;

        var updatedProduct = await _productRepository.UpdateAsync(product);
        return MapToDto(updatedProduct);
    }

    public async Task DeleteProductAsync(int id)
    {
        await _productRepository.DeleteAsync(id);
    }

    public async Task<ProductDto?> GetProductBySkuAsync(string sku)
    {
        var product = await _productRepository.GetBySkuAsync(sku);
        return product == null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        var products = await _productRepository.SearchByNameAsync(searchTerm);
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(PriceRangeDto priceRange)
    {
        var products = await _productRepository.GetByPriceRangeAsync(priceRange.MinPrice, priceRange.MaxPrice);
        return products.Select(MapToDto);
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