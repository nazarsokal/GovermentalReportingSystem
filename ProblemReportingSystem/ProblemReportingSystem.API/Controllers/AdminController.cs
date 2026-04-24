using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProblemReportingSystem.API.Contracts.Response;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;

namespace ProblemReportingSystem.API.Controllers;

/// <summary>
/// API controller for administrative operations.
/// Manages system-wide configurations, bulk data imports, and administrative tasks.
/// Requires authentication and admin role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ICityCouncilService _cityCouncilService;
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ICityCouncilService cityCouncilService,
        IAdminService adminService,
        ILogger<AdminController> logger)
    {
        _cityCouncilService = cityCouncilService ?? throw new ArgumentNullException(nameof(cityCouncilService));
        _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region User Management

    /// <summary>
    /// GET /api/admin/users
    /// Retrieves all users in the system with their role and status information.
    /// </summary>
    /// <returns>List of all users with roles and status</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(UsersListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _adminService.GetUsersAsync();
            var userList = users.ToList();

            _logger.LogInformation($"Retrieved {userList.Count} users");

            return Ok(new UsersListResponse
            {
                Success = true,
                Message = $"Successfully retrieved {userList.Count} user(s)",
                Users = userList.Select(u => new UserListItemDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    AddressId = u.AddressId
                }).ToList(),
                TotalCount = userList.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving users: {ex.Message}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving users"
            });
        }
    }

    #endregion

    #region City Council Management

    /// <summary>
    /// POST /api/admin/city-councils
    /// Creates a new city council in the system.
    /// </summary>
    /// <param name="createCityCouncilDto">The city council data to create</param>
    /// <returns>The created city council ID</returns>
    /// <response code="201">City council created successfully</response>
    /// <response code="400">Invalid city council data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost("city-councils")]
    [ProducesResponseType(typeof(CreateCityCouncilResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCityCouncil([FromBody] CreateCityCouncilDto createCityCouncilDto)
    {
        if (createCityCouncilDto == null)
        {
            _logger.LogWarning("Null city council data provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "City council data cannot be null"
            });
        }

        if (string.IsNullOrWhiteSpace(createCityCouncilDto.Name))
        {
            _logger.LogWarning("Empty city council name provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "City council name is required"
            });
        }

        if (createCityCouncilDto.HeadUserId == Guid.Empty)
        {
            _logger.LogWarning("Empty head user ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Head user ID is required"
            });
        }

        try
        {
            var councilId = await _cityCouncilService.CreateCityCouncilAsync(createCityCouncilDto);

            _logger.LogInformation($"City council created successfully with ID: {councilId}, Name: {createCityCouncilDto.Name}");

            return CreatedAtAction(nameof(GetCityCouncilById), new { councilId }, new CreateCityCouncilResponse
            {
                Success = true,
                Message = "City council created successfully",
                CouncilId = councilId,
                CouncilName = createCityCouncilDto.Name
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError($"Invalid operation: {ex.Message}");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = ex.Message
            });
        }

        catch (ArgumentException ex)
        {
            _logger.LogError($"Invalid argument: {ex.Message}");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating city council: {ex.Message}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while creating the city council"
            });
        }
    }

    /// <summary>
    /// POST /api/admin/city-councils/load-csv
    /// Loads city councils from a CSV file and imports them into the system.
    /// The CSV file must contain the following columns:
    /// - Name (required): The name of the city council
    /// - ContactEmail (optional): The contact email of the council
    /// 
    /// Example CSV format:
    /// Name,ContactEmail
    /// Kyiv City Council,contact@kyiv.ua
    /// Lviv City Council,info@lviv.ua
    /// </summary>
    /// <param name="file">CSV file containing city council data</param>
    /// <returns>List of successfully imported city councils</returns>
    /// <response code="200">City councils imported successfully</response>
    /// <response code="400">Invalid CSV file format or content</response>
    /// <response code="401">Unauthorized - user is not authenticated</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost("city-councils/load-csv")]
    [ProducesResponseType(typeof(CityCouncilLoadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoadCityCouncilsFromCsv(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            _logger.LogWarning("Empty or null CSV file provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "CSV file is required and cannot be empty"
            });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning($"Invalid file type: {file.FileName}");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Only CSV files are accepted. Please provide a file with .csv extension"
            });
        }

        // Check file size (max 5MB)
        const long maxFileSize = 5 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            _logger.LogWarning($"File size exceeds limit: {file.Length} bytes");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "File size cannot exceed 5MB"
            });
        }

        try
        {
            var loadedCouncils = await _cityCouncilService.LoadCityCouncilsFromCsvAsync(file);
            var councilList = loadedCouncils.ToList();

            _logger.LogInformation($"Successfully loaded {councilList.Count} city councils from CSV file");

            return Ok(new CityCouncilLoadResponse
            {
                Success = true,
                Message = $"Successfully imported {councilList.Count} city council(s)",
                LoadedCouncils = councilList.Select(c => new CityCouncilResponseDto
                {
                    CouncilId = c.CouncilId,
                    Name = c.Name,
                    ContactEmail = c.ContactEmail,
                    AddressId = c.AddressId
                }).ToList(),
                TotalCount = councilList.Count
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError($"Invalid argument: {ex.Message}");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError($"Invalid operation: {ex.Message}");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error while loading CSV: {ex.Message}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An unexpected error occurred while processing the CSV file"
            });
        }
    }

    /// <summary>
    /// GET /api/admin/city-councils
    /// Retrieves all city councils in the system with their address information (city, district, oblast).
    /// </summary>
    /// <returns>List of all city councils with address details</returns>
    /// <response code="200">City councils retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("city-councils")]
    [ProducesResponseType(typeof(List<Application.DTOs.CityCouncilWithAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCityCouncils()
    {
        var councils = await _cityCouncilService.GetAllCityCouncilsWithAddressAsync();
        var councilList = councils.ToList();

        _logger.LogInformation($"Retrieved {councilList.Count} city councils");
        return Ok(councilList);
    }

    /// <summary>
    /// GET /api/admin/city-councils/{councilId}
    /// Retrieves detailed information about a specific city council including employees and polls.
    /// </summary>
    /// <param name="councilId">The ID of the city council</param>
    /// <returns>Detailed city council information</returns>
    /// <response code="200">City council retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="404">City council not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("city-councils/{councilId}")]
    [ProducesResponseType(typeof(CityCouncilDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCityCouncilById(Guid councilId)
    {
        if (councilId == Guid.Empty)
        {
            _logger.LogWarning("Invalid council ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid council ID"
            });
        }

        var council = await _cityCouncilService.GetCityCouncilByIdAsync(councilId);
        if (council == null)
        {
            _logger.LogWarning($"City council with ID {councilId} not found");
            return NotFound(new ErrorResponse
            {
                Success = false,
                Message = $"City council with ID {councilId} not found"
            });
        }

        _logger.LogInformation($"Retrieved city council details for {councilId}");
        return Ok(council);
    }

    #endregion
}
