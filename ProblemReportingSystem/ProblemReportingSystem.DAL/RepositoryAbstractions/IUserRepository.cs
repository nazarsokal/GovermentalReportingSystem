using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.DAL.RepositoryAbstractions;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository : IProblemReportingSystemRepository<User>
{
    /// <summary>
    /// Get user by email address
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by Google Auth ID
    /// </summary>
    /// <param name="googleAuthId">The Google authentication ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByGoogleAuthIdAsync(string googleAuthId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user exists by email
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists, false otherwise</returns>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user exists by Google Auth ID
    /// </summary>
    /// <param name="googleAuthId">The Google authentication ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists, false otherwise</returns>
    Task<bool> ExistsByGoogleAuthIdAsync(string googleAuthId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by email excluding a specific user ID
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="excludeUserId">User ID to exclude from search</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByEmailExcludingAsync(string email, Guid excludeUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by Google Auth ID excluding a specific user ID
    /// </summary>
    /// <param name="googleAuthId">The Google authentication ID</param>
    /// <param name="excludeUserId">User ID to exclude from search</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByGoogleAuthIdExcludingAsync(string googleAuthId, Guid excludeUserId, CancellationToken cancellationToken = default);
}
