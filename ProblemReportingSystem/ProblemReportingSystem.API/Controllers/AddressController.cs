using Microsoft.AspNetCore.Mvc;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;

namespace ProblemReportingSystem.API.Controllers;

/// <summary>
/// API controller for address-related operations.
/// Provides endpoints to retrieve oblast and district information from the database.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly ILogger<AddressController> _logger;

    public AddressController(
        IAddressService addressService,
        ILogger<AddressController> logger)
    {
        _addressService = addressService ?? throw new ArgumentNullException(nameof(addressService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// GET /api/Address/oblasts
    /// Retrieves all unique oblasts (regions) from the database.
    /// </summary>
    /// <returns>List of oblast names sorted alphabetically</returns>
    /// <response code="200">Successfully retrieved oblasts</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("oblasts")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOblasts()
    {
        try
        {
            _logger.LogInformation("Fetching all oblasts");
            var oblasts = await _addressService.GetAllOblastsAsync();
            return Ok(oblasts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving oblasts: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An error occurred while retrieving oblasts",
                error = ex.Message
            });
        }
    }
    
    /// <summary>
    /// GET /api/Address/cities
    /// Retrieves all unique cities from the database.
    /// </summary>
    /// <returns>List of cities names sorted alphabetically</returns>
    /// <response code="200">Successfully retrieved oblasts</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("cities")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCities()
    {
        try
        {
            _logger.LogInformation("Fetching all cities");
            var oblasts = await _addressService.GetAllCitiesAsync();
            return Ok(oblasts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving oblasts: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An error occurred while retrieving oblasts",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// GET /api/Address/districts
    /// Retrieves all unique districts from the database.
    /// </summary>
    /// <returns>List of district names sorted alphabetically</returns>
    /// <response code="200">Successfully retrieved districts</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("districts")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDistricts()
    {
        try
        {
            _logger.LogInformation("Fetching all districts");
            var districts = await _addressService.GetAllDistrictsAsync();
            return Ok(districts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving districts: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An error occurred while retrieving districts",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// GET /api/Address/districts/{oblast}
    /// Retrieves all unique districts for a specific oblast.
    /// </summary>
    /// <param name="oblast">The oblast name to filter districts by</param>
    /// <returns>List of district names in the oblast sorted alphabetically</returns>
    /// <response code="200">Successfully retrieved districts for oblast</response>
    /// <response code="400">Oblast parameter is invalid or empty</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("districts/{oblast}")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDistrictsByCity(string oblast)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(oblast))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Oblast parameter is required"
                });
            }

            _logger.LogInformation($"Fetching districts for oblast: {oblast}");
            var districts = await _addressService.GetDistrictsByOblastAsync(oblast);
            return Ok(districts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving districts for oblast {oblast}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An error occurred while retrieving districts",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// GET /api/Address/districts/{city}
    /// Retrieves all unique districts for a specific city.
    /// </summary>
    /// <param name="oblast"></param>
    /// <returns>List of district names in the city sorted alphabetically</returns>
    /// <response code="200">Successfully retrieved districts for city</response>
    /// <response code="400">Oblast parameter is invalid or empty</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("cities/{oblast}")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCityByOblast(string oblast)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(oblast))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Oblast parameter is required"
                });
            }

            _logger.LogInformation($"Fetching districts for city: {oblast}");
            var districts = await _addressService.GetCitiesByOblastAsync(oblast);
            return Ok(districts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving districts for city {oblast}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An error occurred while retrieving districts",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// GET /api/Address/districts/coordinates/all
    /// Retrieves all unique districts with their coordinates.
    /// Prioritizes coordinates from addresses where city councils are located.
    /// </summary>
    /// <returns>List of districts with their coordinates sorted alphabetically by district name</returns>
    /// <response code="200">Successfully retrieved districts with coordinates</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("districts/coordinates/all")]
    [ProducesResponseType(typeof(List<DistrictCoordinatesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDistrictsWithCoordinates()
    {
        try
        {
            _logger.LogInformation("Fetching all districts with coordinates");
            var districtsWithCoordinates = await _addressService.GetDistrictsWithCoordinatesAsync();
            return Ok(districtsWithCoordinates);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving districts with coordinates: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "An error occurred while retrieving districts with coordinates",
                error = ex.Message
            });
        }
    }
}
