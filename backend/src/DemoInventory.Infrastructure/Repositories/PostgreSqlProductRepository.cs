using DemoInventory.Domain.Entities;
using DemoInventory.Domain.Interfaces;
using DemoInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DemoInventory.Infrastructure.Repositories;

public class PostgreSqlProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public PostgreSqlProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product> AddAsync(Product entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        
        _context.Products.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

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

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<Product?> GetByNameAsync(string name)
    {
        return await _context.Products.FirstOrDefaultAsync(p => EF.Functions.ILike(p.Name, name));
    }

    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _context.Products
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
    {
        return await _context.Products
            .Where(p => p.Name.Contains(name))
            .ToListAsync();
    }
}