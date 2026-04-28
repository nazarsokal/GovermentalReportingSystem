using Microsoft.EntityFrameworkCore;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.DAL.Repositories;

/// <summary>
/// Generic repository implementation for CRUD operations on entities
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public class ProblemReportingSystemRepository<T> : IProblemReportingSystemRepository<T> where T : class
{
    private readonly ProblemReportingSystemDbContext _context;
    private readonly DbSet<T> _dbSet;

    public ProblemReportingSystemRepository(ProblemReportingSystemDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// Get all entities
    /// </summary>
    /// <returns>IQueryable of entities for further filtering/pagination</returns>
    public IQueryable<T> GetAll()
    {
        return _dbSet.AsNoTracking();
    }

    /// <summary>
    /// Get entity by primary key
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <returns>The entity if found, null otherwise</returns>
    public async Task<T?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));

        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <returns>The created entity</returns>
    public async Task<T> CreateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entry = await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entry.Entity;
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    public async Task<T> UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Delete an entity by primary key
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));

        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Delete an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    public async Task<bool> DeleteAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    /// <summary>
    /// Check if entity exists by primary key
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <returns>True if entity exists, false otherwise</returns>
    public async Task<bool> ExistsAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));

        return await _dbSet.FindAsync(id) != null;
    }

    /// <summary>
    /// Get count of all entities
    /// </summary>
    /// <returns>The count of entities</returns>
    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

     /// <summary>
     /// Save changes to the database
     /// </summary>
     /// <returns>The number of entities affected</returns>
     public async Task<int> SaveChangesAsync()
     {
         return await _context.SaveChangesAsync();
     }

     /// <summary>
     /// Detach an entity from the context to free up tracking
     /// Useful when loading many entities in a loop to prevent tracking conflicts
     /// </summary>
     /// <param name="entity">The entity to detach</param>
     public void DetachEntity(T entity)
     {
         if (entity == null)
             return;

         var entry = _context.Entry(entity);
         if (entry != null)
         {
             entry.State = EntityState.Detached;
         }
     }

     /// <summary>
     /// Clear all tracked entities from the context
     /// Use with caution as this disconnects all entities
     /// </summary>
     public void ClearTrackedEntities()
     {
         _context.ChangeTracker.Clear();
     }
}

