using DemoInventory.Application.DTOs;

namespace DemoInventory.Application.Interfaces;

/// <summary>
/// Defines the contract for product service operations
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves a product by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the product</param>
    /// <returns>The product DTO if found, otherwise null</returns>
    Task<ProductDto?> GetProductByIdAsync(int id);
    
    /// <summary>
    /// Retrieves all products
    /// </summary>
    /// <returns>A collection of all product DTOs</returns>
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    
    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="createProductDto">The product creation data</param>
    /// <returns>The created product DTO</returns>
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
    
    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The unique identifier of the product to update</param>
    /// <param name="updateProductDto">The product update data</param>
    /// <returns>The updated product DTO</returns>
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
    
    /// <summary>
    /// Deletes a product by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteProductAsync(int id);
    
    /// <summary>
    /// Retrieves a product by its Stock Keeping Unit (SKU)
    /// </summary>
    /// <param name="sku">The SKU of the product to retrieve</param>
    /// <returns>The product DTO if found, otherwise null</returns>
    Task<ProductDto?> GetProductBySkuAsync(string sku);
    
    /// <summary>
    /// Retrieves a product by its exact name
    /// </summary>
    /// <param name="name">The name of the product to retrieve</param>
    /// <returns>The product DTO if found, otherwise null</returns>
    Task<ProductDto?> GetProductByNameAsync(string name);
    
    /// <summary>
    /// Searches for products by name using partial matching
    /// </summary>
    /// <param name="searchTerm">The search term to match against product names</param>
    /// <returns>A collection of product DTOs whose names contain the search term</returns>
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
    
    /// <summary>
    /// Retrieves products within a specified price range
    /// </summary>
    /// <param name="priceRange">The price range criteria</param>
    /// <returns>A collection of product DTOs within the specified price range</returns>
    Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(PriceRangeDto priceRange);
}