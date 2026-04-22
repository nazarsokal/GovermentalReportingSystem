namespace ProblemReportingSystem.Application.ServiceAbstractions;

using ProblemReportingSystem.Application.DTOs;

/// <summary>
/// Service interface for Address-related operations
/// </summary>
public interface IAddressService
{
    /// <summary>
    /// Get all unique oblasts from addresses in the database
    /// </summary>
    /// <returns>List of oblast names sorted alphabetically</returns>
    Task<List<string>> GetAllOblastsAsync();

    /// <summary>
    /// Get all unique districts from addresses in the database
    /// </summary>
    /// <returns>List of district names sorted alphabetically</returns>
    Task<List<string>> GetAllDistrictsAsync();

    /// <summary>
    /// Get all unique districts for a specific oblast
    /// </summary>
    /// <param name="oblast">The oblast name</param>
    /// <returns>List of district names in the oblast sorted alphabetically</returns>
    Task<List<string>> GetDistrictsByOblastAsync(string oblast);

    /// <summary>
    /// Get all unique districts with their coordinates
    /// Prioritizes coordinates from addresses where city councils are located
    /// </summary>
    /// <returns>List of districts with coordinates sorted alphabetically by district name</returns>
    Task<List<DistrictCoordinatesDto>> GetDistrictsWithCoordinatesAsync();
}
