using ProblemReportingSystem.DAL.Entities;

namespace ProblemReportingSystem.DAL.RepositoryAbstractions;

/// <summary>
/// Repository interface for Address entity operations
/// </summary>
public interface IAddressRepository : IProblemReportingSystemRepository<Address>
{
    /// <summary>
    /// Get all unique oblasts from addresses
    /// </summary>
    /// <returns>List of oblast names</returns>
    Task<List<string>> GetAllOblastsAsync();

    /// <summary>
    /// Get all unique districts from addresses
    /// </summary>
    /// <returns>List of district names</returns>
    Task<List<string>> GetAllDistrictsAsync();

    /// <summary>
    /// Get all unique districts for a specific oblast
    /// </summary>
    /// <param name="oblast">The oblast name</param>
    /// <returns>List of district names in the oblast</returns>
    Task<List<string>> GetDistrictsByOblastAsync(string oblast);

    /// <summary>
    /// Get all unique districts with their coordinates
    /// Prioritizes coordinates from addresses where city councils are located
    /// </summary>
    /// <returns>List of districts with coordinates</returns>
    Task<List<(string District, decimal Latitude, decimal Longitude)>> GetDistrictsWithCoordinatesAsync();
}
