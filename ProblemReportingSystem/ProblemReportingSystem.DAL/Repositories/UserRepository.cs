using Microsoft.EntityFrameworkCore;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.DAL.Repositories;

/// <summary>
/// Repository implementation for User entity operations
/// </summary>
public class UserRepository : ProblemReportingSystemRepository<User>, IUserRepository
{
    private readonly ProblemReportingSystemDbContext _context;

    public UserRepository(ProblemReportingSystemDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Get user by ID with all related entities including roles
    /// </summary>
    public async Task<User?> GetByIdWithRolesAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(id));

        return await _context.Users
            .Include(u => u.Address)
            .Include(u => u.Admin)
            .Include(u => u.CouncilEmployee)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    /// <summary>
    /// Get user by email address with all related entities
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return await _context.Users
            .Include(u => u.Address)
            .Include(u => u.Admin)
            .Include(u => u.CouncilEmployee)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>
    /// Get user by Google Auth ID with all related entities
    /// </summary>
    public async Task<User?> GetByGoogleAuthIdAsync(string googleAuthId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(googleAuthId))
            throw new ArgumentException("Google Auth ID cannot be empty", nameof(googleAuthId));

        return await _context.Users
            .Include(u => u.Address)
            .Include(u => u.Admin)
            .Include(u => u.CouncilEmployee)
            .FirstOrDefaultAsync(u => u.GoogleAuthId == googleAuthId, cancellationToken);
    }

    /// <summary>
    /// Check if user exists by email
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>
    /// Check if user exists by Google Auth ID
    /// </summary>
    public async Task<bool> ExistsByGoogleAuthIdAsync(string googleAuthId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(googleAuthId))
            throw new ArgumentException("Google Auth ID cannot be empty", nameof(googleAuthId));

        return await _context.Users
            .AnyAsync(u => u.GoogleAuthId == googleAuthId, cancellationToken);
    }

    /// <summary>
    /// Get user by email excluding a specific user ID with all related entities
    /// </summary>
    public async Task<User?> GetByEmailExcludingAsync(string email, Guid excludeUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (excludeUserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(excludeUserId));

        return await _context.Users
            .Include(u => u.Address)
            .Include(u => u.Admin)
            .Include(u => u.CouncilEmployee)
            .FirstOrDefaultAsync(u => u.Email == email && u.UserId != excludeUserId, cancellationToken);
    }

    /// <summary>
    /// Get user by Google Auth ID excluding a specific user ID with all related entities
    /// </summary>
    public async Task<User?> GetByGoogleAuthIdExcludingAsync(string googleAuthId, Guid excludeUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(googleAuthId))
            throw new ArgumentException("Google Auth ID cannot be empty", nameof(googleAuthId));

        if (excludeUserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(excludeUserId));

        return await _context.Users
            .Include(u => u.Address)
            .Include(u => u.Admin)
            .Include(u => u.CouncilEmployee)
            .FirstOrDefaultAsync(u => u.GoogleAuthId == googleAuthId && u.UserId != excludeUserId, cancellationToken);
    }
}
