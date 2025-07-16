using DemoInventory.Domain.Entities;
using DemoInventory.Domain.Interfaces;

namespace DemoInventory.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of the product repository for testing and development purposes
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;

    /// <summary>
    /// Retrieves a product by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the product</param>
    /// <returns>The product if found, otherwise null</returns>
    public Task<Product?> GetByIdAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    /// <summary>
    /// Retrieves all products from the in-memory collection
    /// </summary>
    /// <returns>A collection of all products</returns>
    public Task<IEnumerable<Product>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Product>>(_products.ToList());
    }

    /// <summary>
    /// Adds a new product to the in-memory collection
    /// </summary>
    /// <param name="entity">The product entity to add</param>
    /// <returns>The added product with assigned ID</returns>
    public Task<Product> AddAsync(Product entity)
    {
        entity.Id = _nextId++;
        _products.Add(entity);
        return Task.FromResult(entity);
    }

    /// <summary>
    /// Updates an existing product in the in-memory collection
    /// </summary>
    /// <param name="entity">The product entity with updated values</param>
    /// <returns>The updated product</returns>
    /// <exception cref="InvalidOperationException">Thrown when the product is not found</exception>
    public Task<Product> UpdateAsync(Product entity)
    {
        var existingProduct = _products.FirstOrDefault(p => p.Id == entity.Id);
        if (existingProduct == null)
            throw new InvalidOperationException($"Product with ID {entity.Id} not found");

        existingProduct.Name = entity.Name;
        existingProduct.Description = entity.Description;
        existingProduct.Price = entity.Price;
        existingProduct.QuantityInStock = entity.QuantityInStock;
        existingProduct.UpdatedAt = entity.UpdatedAt;

        return Task.FromResult(existingProduct);
    }

    /// <summary>
    /// Deletes a product from the in-memory collection by its ID
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task DeleteAsync(int id)
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
    /// <returns>The product if found, otherwise null</returns>
    public Task<Product?> GetBySkuAsync(string sku)
    {
        var product = _products.FirstOrDefault(p => p.SKU == sku);
        return Task.FromResult(product);
    }

    /// <summary>
    /// Retrieves a product by its exact name (case-insensitive)
    /// </summary>
    /// <param name="name">The name of the product to retrieve</param>
    /// <returns>The product if found, otherwise null</returns>
    public Task<Product?> GetByNameAsync(string name)
    {
        var product = _products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(product);
    }

    /// <summary>
    /// Retrieves products within a specified price range
    /// </summary>
    /// <param name="minPrice">The minimum price (inclusive)</param>
    /// <param name="maxPrice">The maximum price (inclusive)</param>
    /// <returns>A collection of products within the specified price range</returns>
    public Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var products = _products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList();
        return Task.FromResult<IEnumerable<Product>>(products);
    }

    /// <summary>
    /// Searches for products by name using partial matching (case-insensitive)
    /// </summary>
    /// <param name="name">The search term to match against product names</param>
    /// <returns>A collection of products whose names contain the search term</returns>
    public Task<IEnumerable<Product>> SearchByNameAsync(string name)
    {
        var products = _products.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult<IEnumerable<Product>>(products);
    }
}