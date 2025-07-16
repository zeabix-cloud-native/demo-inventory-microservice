using DemoInventory.Domain.Entities;
using DemoInventory.Domain.Interfaces;
using DemoInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DemoInventory.Infrastructure.Repositories;

/// <summary>
/// PostgreSQL implementation of the product repository using Entity Framework Core
/// </summary>
public class PostgreSqlProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public PostgreSqlProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a product by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the product</param>
    /// <returns>The product if found, otherwise null</returns>
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    /// <summary>
    /// Retrieves all products from the database
    /// </summary>
    /// <returns>A collection of all products</returns>
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    /// <summary>
    /// Adds a new product to the database
    /// </summary>
    /// <param name="entity">The product entity to add</param>
    /// <returns>The added product with updated timestamps and ID</returns>
    public async Task<Product> AddAsync(Product entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        
        _context.Products.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Updates an existing product in the database
    /// </summary>
    /// <param name="entity">The product entity with updated values</param>
    /// <returns>The updated product</returns>
    /// <exception cref="InvalidOperationException">Thrown when the product is not found</exception>
    public async Task<Product> UpdateAsync(Product entity)
    {
        var existingProduct = await _context.Products.FindAsync(entity.Id);
        if (existingProduct == null)
            throw new InvalidOperationException($"Product with ID {entity.Id} not found");

        existingProduct.Name = entity.Name;
        existingProduct.Description = entity.Description;
        existingProduct.Price = entity.Price;
        existingProduct.QuantityInStock = entity.QuantityInStock;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingProduct;
    }

    /// <summary>
    /// Deletes a product from the database by its ID
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Retrieves a product by its Stock Keeping Unit (SKU)
    /// </summary>
    /// <param name="sku">The SKU of the product to retrieve</param>
    /// <returns>The product if found, otherwise null</returns>
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.SKU == sku);
    }

    /// <summary>
    /// Retrieves a product by its exact name (case-insensitive)
    /// </summary>
    /// <param name="name">The name of the product to retrieve</param>
    /// <returns>The product if found, otherwise null</returns>
    public async Task<Product?> GetByNameAsync(string name)
    {
        return await _context.Products.FirstOrDefaultAsync(p => EF.Functions.ILike(p.Name, name));
    }

    /// <summary>
    /// Retrieves products within a specified price range
    /// </summary>
    /// <param name="minPrice">The minimum price (inclusive)</param>
    /// <param name="maxPrice">The maximum price (inclusive)</param>
    /// <returns>A collection of products within the specified price range</returns>
    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _context.Products
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .ToListAsync();
    }

    /// <summary>
    /// Searches for products by name using partial matching
    /// </summary>
    /// <param name="name">The search term to match against product names</param>
    /// <returns>A collection of products whose names contain the search term</returns>
    public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
    {
        // Use parameterized query with EF.Functions for secure search
        var searchPattern = $"%{name.Replace("%", "\\%").Replace("_", "\\_")}%";
        return await _context.Products
            .Where(p => EF.Functions.ILike(p.Name, searchPattern))
            .ToListAsync();
    }
}