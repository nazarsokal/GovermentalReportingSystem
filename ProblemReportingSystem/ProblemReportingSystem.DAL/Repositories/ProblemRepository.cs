using Microsoft.EntityFrameworkCore;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.DAL.Repositories;

public class ProblemRepository : ProblemReportingSystemRepository<Problem>, IProblemRepository
{
    private readonly ProblemReportingSystemDbContext _context;

    public ProblemRepository(ProblemReportingSystemDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets all problems with their related details (Address, Category, Photos).
    /// </summary>
    public async Task<IEnumerable<Problem>> GetAllProblemsAsync()
    {
        return await _context.Problems
            .Include(p => p.Address)
            .Include(p => p.Category)
            .Include(p => p.ProblemPhotos)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a problem with all its related details (Address, Category, Photos, Appeals).
    /// </summary>
    public async Task<Problem?> GetProblemWithDetailsAsync(Guid problemId)
    {
        return await _context.Problems
            .Include(p => p.Address)
            .Include(p => p.Category)
            .Include(p => p.ProblemPhotos)
            .Include(p => p.Appeals)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId);
    }

    /// <summary>
    /// Gets all problems reported by a specific user.
    /// </summary>
    public async Task<IEnumerable<Problem>> GetUserProblemsAsync(Guid userId)
    {
        return await _context.Problems
            .Where(p => p.Address != null) // Filter problems that have associated addresses
            .Include(p => p.Address)
            .Include(p => p.Category)
            .Include(p => p.ProblemPhotos)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all problems with a specific status.
    /// </summary>
    public async Task<IEnumerable<Problem>> GetProblemsByStatusAsync(string status)
    {
        return await _context.Problems
            .Where(p => p.Status == status)
            .Include(p => p.Address)
            .Include(p => p.Category)
            .Include(p => p.ProblemPhotos)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all problems in a specific category.
    /// </summary>
    public async Task<IEnumerable<Problem>> GetProblemsByCategoryAsync(Guid categoryId)
    {
        return await _context.Problems
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Address)
            .Include(p => p.Category)
            .Include(p => p.ProblemPhotos)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all problems within geographic bounds (latitude and longitude ranges).
    /// Useful for map-based queries.
    /// </summary>
    public async Task<IEnumerable<Problem>> GetProblemsInBoundsAsync(decimal minLat, decimal maxLat, decimal minLng, decimal maxLng)
    {
        return await _context.Problems
            .Include(p => p.Address)
            .Include(p => p.Category)
            .Include(p => p.ProblemPhotos)
            .Where(p => p.Address != null &&
                        p.Address.Latitude >= minLat &&
                        p.Address.Latitude <= maxLat &&
                        p.Address.Longitude >= minLng &&
                        p.Address.Longitude <= maxLng)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all problems assigned to a specific council employee.
    /// </summary>
    public async Task<IEnumerable<Problem>> GetAssignedProblemsAsync(Guid employeeId)
    {
        return await _context.Problems
            .Include(p => p.Address)
            .Include(p => p.Category)
            .Include(p => p.ProblemPhotos)
            .Include(p => p.Appeals)
                .ThenInclude(a => a.AssignedEmployee)
            .Where(p => p.Appeals.Any(a => a.AssignedEmployeeId == employeeId))
            .ToListAsync();
    }
}