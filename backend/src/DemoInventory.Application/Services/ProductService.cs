using DemoInventory.Application.DTOs;
using DemoInventory.Application.Interfaces;
using DemoInventory.Domain.Entities;
using DemoInventory.Domain.Interfaces;

namespace DemoInventory.Application.Services;

/// <summary>
/// Application service for managing products with business logic and validation
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// Retrieves a product by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the product</param>
    /// <returns>The product DTO if found, otherwise null</returns>
    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product == null ? null : MapToDto(product);
    }

    /// <summary>
    /// Retrieves all products from the repository
    /// </summary>
    /// <returns>A collection of all product DTOs</returns>
    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    /// <summary>
    /// Creates a new product with SKU uniqueness validation
    /// </summary>
    /// <param name="createProductDto">The product creation data</param>
    /// <returns>The created product DTO</returns>
    /// <exception cref="ArgumentException">Thrown when the SKU already exists</exception>
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

    /// <summary>
    /// Updates an existing product with validation
    /// </summary>
    /// <param name="id">The unique identifier of the product to update</param>
    /// <param name="updateProductDto">The product update data</param>
    /// <returns>The updated product DTO</returns>
    /// <exception cref="InvalidOperationException">Thrown when the product is not found</exception>
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

    /// <summary>
    /// Deletes a product by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task DeleteProductAsync(int id)
    {
        await _productRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Retrieves a product by its Stock Keeping Unit (SKU)
    /// </summary>
    /// <param name="sku">The SKU of the product to retrieve</param>
    /// <returns>The product DTO if found, otherwise null</returns>
    public async Task<ProductDto?> GetProductBySkuAsync(string sku)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("SKU cannot be null or empty.", nameof(sku));
        }

        if (sku.Length > 50)
        {
            throw new ArgumentException("SKU cannot exceed 50 characters.", nameof(sku));
        }

        var product = await _productRepository.GetBySkuAsync(sku.Trim());
        return product == null ? null : MapToDto(product);
    }

    /// <summary>
    /// Retrieves a product by its exact name (case-insensitive)
    /// </summary>
    /// <param name="name">The name of the product to retrieve</param>
    /// <returns>The product DTO if found, otherwise null</returns>
    public async Task<ProductDto?> GetProductByNameAsync(string name)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name cannot be null or empty.", nameof(name));
        }

        if (name.Length > 200)
        {
            throw new ArgumentException("Product name cannot exceed 200 characters.", nameof(name));
        }

        var product = await _productRepository.GetByNameAsync(name.Trim());
        return product == null ? null : MapToDto(product);
    }

    /// <summary>
    /// Searches for products by name using partial matching
    /// </summary>
    /// <param name="searchTerm">The search term to match against product names</param>
    /// <returns>A collection of product DTOs whose names contain the search term</returns>
    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        // Input validation and sanitization
        if (string.IsNullOrEmpty(searchTerm))
        {
            return await GetAllProductsAsync();
        }

        // Limit search term length for security
        if (searchTerm.Length > 200)
        {
            throw new ArgumentException("Search term cannot exceed 200 characters.", nameof(searchTerm));
        }

        var products = await _productRepository.SearchByNameAsync(searchTerm.Trim());
        return products.Select(MapToDto);
    }

    /// <summary>
    /// Retrieves products within a specified price range
    /// </summary>
    /// <param name="priceRange">The price range criteria</param>
    /// <returns>A collection of product DTOs within the specified price range</returns>
    public async Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(PriceRangeDto priceRange)
    {
        // Input validation
        if (priceRange == null)
        {
            throw new ArgumentNullException(nameof(priceRange), "Price range cannot be null.");
        }

        if (priceRange.MinPrice < 0)
        {
            throw new ArgumentException("Minimum price cannot be negative.", nameof(priceRange.MinPrice));
        }

        if (priceRange.MaxPrice < 0)
        {
            throw new ArgumentException("Maximum price cannot be negative.", nameof(priceRange.MaxPrice));
        }

        if (priceRange.MinPrice > priceRange.MaxPrice)
        {
            throw new ArgumentException("Minimum price cannot be greater than maximum price.", nameof(priceRange));
        }

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