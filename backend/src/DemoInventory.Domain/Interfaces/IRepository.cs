namespace DemoInventory.Domain.Interfaces;

/// <summary>
/// Generic repository interface defining basic CRUD operations
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Retrieves an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the entity</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<T?> GetByIdAsync(int id);
    
    /// <summary>
    /// Retrieves all entities
    /// </summary>
    /// <returns>A collection of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    Task<T> AddAsync(T entity);
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity with updated values</param>
    /// <returns>The updated entity</returns>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>
    /// Deletes an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteAsync(int id);
}