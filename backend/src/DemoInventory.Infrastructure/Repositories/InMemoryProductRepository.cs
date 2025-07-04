using DemoInventory.Domain.Entities;
using DemoInventory.Domain.Interfaces;

namespace DemoInventory.Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;

    public Task<Product?> GetByIdAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Product>>(_products.ToList());
    }

    public Task<Product> AddAsync(Product entity)
    {
        entity.Id = _nextId++;
        _products.Add(entity);
        return Task.FromResult(entity);
    }

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

    public Task DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
        }
        return Task.CompletedTask;
    }

    public Task<Product?> GetBySkuAsync(string sku)
    {
        var product = _products.FirstOrDefault(p => p.SKU == sku);
        return Task.FromResult(product);
    }

    public Task<Product?> GetByNameAsync(string name)
    {
        var product = _products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var products = _products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList();
        return Task.FromResult<IEnumerable<Product>>(products);
    }

    public Task<IEnumerable<Product>> SearchByNameAsync(string name)
    {
        var products = _products.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult<IEnumerable<Product>>(products);
    }
}