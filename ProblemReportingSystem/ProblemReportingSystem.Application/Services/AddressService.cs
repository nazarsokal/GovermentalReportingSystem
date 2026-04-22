using Microsoft.Extensions.Logging;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.Application.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly ILogger<AddressService> _logger;

    public AddressService(IAddressRepository addressRepository, ILogger<AddressService> logger)
    {
        _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all unique oblasts from addresses in the database
    /// </summary>
    public async Task<List<string>> GetAllOblastsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all oblasts from database");
            return await _addressRepository.GetAllOblastsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving oblasts: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get all unique districts from addresses in the database
    /// </summary>
    public async Task<List<string>> GetAllDistrictsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all districts from database");
            return await _addressRepository.GetAllDistrictsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving districts: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get all unique districts for a specific oblast
    /// </summary>
    public async Task<List<string>> GetDistrictsByOblastAsync(string oblast)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(oblast))
                throw new ArgumentException("Oblast cannot be null or empty", nameof(oblast));

            _logger.LogInformation($"Retrieving districts for oblast: {oblast}");
            return await _addressRepository.GetDistrictsByOblastAsync(oblast);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving districts for oblast {oblast}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Get all unique districts with their coordinates
    /// Prioritizes coordinates from addresses where city councils are located
    /// </summary>
    public async Task<List<DistrictCoordinatesDto>> GetDistrictsWithCoordinatesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving districts with coordinates from database");
            var districtCoordinates = await _addressRepository.GetDistrictsWithCoordinatesAsync();
            
            return districtCoordinates
                .Select(dc => new DistrictCoordinatesDto
                {
                    District = dc.District,
                    Latitude = dc.Latitude,
                    Longitude = dc.Longitude
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving districts with coordinates: {ex.Message}");
            throw;
        }
    }
}
