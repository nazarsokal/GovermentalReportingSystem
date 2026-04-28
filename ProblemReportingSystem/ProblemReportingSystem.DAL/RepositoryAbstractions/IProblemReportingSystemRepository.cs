namespace ProblemReportingSystem.DAL.RepositoryAbstractions;

/// <summary>
/// Generic repository interface for CRUD operations on entities
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IProblemReportingSystemRepository<T> where T : class
{
    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns>IQueryable of entities for further filtering/pagination</returns>
    IQueryable<T> GetAll();

    /// <summary>
    /// Get entity by primary key
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <returns>The created entity</returns>
    Task<T> CreateAsync(T entity);

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Delete an entity by primary key
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Delete an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteAsync(T entity);

    /// <summary>
    /// Check if entity exists by primary key
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <returns>True if entity exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Get count of all entities
    /// </summary>
    /// <returns>The count of entities</returns>
    Task<int> CountAsync();

     /// <summary>
     /// Save changes to the database
     /// </summary>
     /// <returns>The number of entities affected</returns>
     Task<int> SaveChangesAsync();

     /// <summary>
     /// Detach an entity from the context to free up tracking
     /// Useful when loading many entities in a loop to prevent tracking conflicts
     /// </summary>
     /// <param name="entity">The entity to detach</param>
     void DetachEntity(T entity);

     /// <summary>
     /// Clear all tracked entities from the context
     /// Use with caution as this disconnects all entities
     /// </summary>
     void ClearTrackedEntities();
}