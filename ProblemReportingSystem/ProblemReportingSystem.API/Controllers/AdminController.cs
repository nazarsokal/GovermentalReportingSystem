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

    /// <summary>
    /// GET /api/admin/users/{userId}
    /// Retrieves detailed information about a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve</param>
    /// <returns>User details</returns>
    /// <response code="200">User retrieved successfully</response>
    /// <response code="400">Invalid user ID</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid user ID"
            });
        }

        try
        {
            var user = await _adminService.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                return NotFound(new ErrorResponse
                {
                    Success = false,
                    Message = $"User with ID {userId} not found"
                });
            }

            _logger.LogInformation($"Retrieved user details for {userId}");
            return Ok(user);
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
            _logger.LogError($"Error retrieving user: {ex.Message}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the user"
            });
        }
    }

    /// <summary>
    /// POST /api/admin/users
    /// Creates a new user in the system.
    /// </summary>
    /// <param name="createUserDto">The user data to create</param>
    /// <returns>The created user ID</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid user data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost("users")]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        if (createUserDto == null)
        {
            _logger.LogWarning("Null user data provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "User data cannot be null"
            });
        }

        if (string.IsNullOrWhiteSpace(createUserDto.FullName))
        {
            _logger.LogWarning("Empty full name provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Full name is required"
            });
        }

        if (string.IsNullOrWhiteSpace(createUserDto.Email))
        {
            _logger.LogWarning("Empty email provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Email is required"
            });
        }

        try
        {
            var userId = await _adminService.CreateUserAsync(createUserDto);

            _logger.LogInformation($"User created successfully with ID: {userId}, Email: {createUserDto.Email}");

            return CreatedAtAction(nameof(GetUserById), new { userId }, new CreateUserResponse
            {
                Success = true,
                Message = "User created successfully",
                UserId = userId,
                FullName = createUserDto.FullName,
                Email = createUserDto.Email
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
            _logger.LogError($"Error creating user: {ex.Message}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while creating the user"
            });
        }
    }

    /// <summary>
    /// PUT /api/admin/users/{userId}
    /// Updates an existing user in the system.
    /// </summary>
    /// <param name="userId">The ID of the user to update</param>
    /// <param name="updateUserDto">The updated user data</param>
    /// <returns>The updated user</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Invalid user data or ID mismatch</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpPut("users/{userId}")]
    [ProducesResponseType(typeof(UpdateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto updateUserDto)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid user ID"
            });
        }

        if (updateUserDto == null)
        {
            _logger.LogWarning("Null user data provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "User data cannot be null"
            });
        }

        if (updateUserDto.UserId != userId)
        {
            _logger.LogWarning("User ID mismatch");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "User ID in URL does not match user ID in request body"
            });
        }

        if (string.IsNullOrWhiteSpace(updateUserDto.FullName))
        {
            _logger.LogWarning("Empty full name provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Full name is required"
            });
        }

        if (string.IsNullOrWhiteSpace(updateUserDto.Email))
        {
            _logger.LogWarning("Empty email provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Email is required"
            });
        }

        try
        {
            var updatedUser = await _adminService.UpdateUserAsync(updateUserDto);

            _logger.LogInformation($"User updated successfully with ID: {userId}");

            return Ok(new UpdateUserResponse
            {
                Success = true,
                Message = "User updated successfully",
                User = new UserListItemDto
                {
                    UserId = updatedUser.UserId,
                    FullName = updatedUser.FullName,
                    Email = updatedUser.Email,
                    Role = "User",
                    IsActive = updatedUser.IsActive,
                    AddressId = updatedUser.AddressId
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError($"Invalid operation: {ex.Message}");
            return NotFound(new ErrorResponse
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
            _logger.LogError($"Error updating user: {ex.Message}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while updating the user"
            });
        }
    }

    /// <summary>
    /// DELETE /api/admin/users/{userId}
    /// Deletes a user from the system.
    /// </summary>
    /// <param name="userId">The ID of the user to delete</param>
    /// <returns>Success message</returns>
    /// <response code="200">User deleted successfully</response>
    /// <response code="400">Invalid user ID</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="404">User not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpDelete("users/{userId}")]
    [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid user ID"
            });
        }

        try
        {
            var deleted = await _adminService.DeleteUserAsync(userId);
            
            if (!deleted)
            {
                _logger.LogWarning($"User with ID {userId} not found for deletion");
                return NotFound(new ErrorResponse
                {
                    Success = false,
                    Message = $"User with ID {userId} not found"
                });
            }

            _logger.LogInformation($"User deleted successfully with ID: {userId}");

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "User deleted successfully"
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
            _logger.LogError($"Error deleting user: {ex.Message}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while deleting the user"
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
    [ProducesResponseType(typeof(List<CityCouncilWithAddressDto>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// PUT /api/admin/city-councils/{councilId}
    /// Updates an existing city council.
    /// </summary>
    /// <param name="councilId">The ID of the city council to update</param>
    /// <param name="updateCityCouncilDto">The updated city council data</param>
    /// <returns>The updated city council</returns>
    /// <response code="200">City council updated successfully</response>
    /// <response code="400">Invalid city council data or ID mismatch</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="404">City council not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpPut("city-councils/{councilId}")]
    [ProducesResponseType(typeof(UpdateCityCouncilResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCityCouncil(Guid councilId, [FromBody] UpdateCityCouncilDto updateCityCouncilDto)
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

        if (updateCityCouncilDto == null)
        {
            _logger.LogWarning("Null city council data provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "City council data cannot be null"
            });
        }

        if (updateCityCouncilDto.CouncilId != councilId)
        {
            _logger.LogWarning("Council ID mismatch");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Council ID in URL does not match council ID in request body"
            });
        }

        if (string.IsNullOrWhiteSpace(updateCityCouncilDto.Name))
        {
            _logger.LogWarning("Empty city council name provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "City council name is required"
            });
        }

        try
        {
            var updatedCouncil = await _cityCouncilService.UpdateCityCouncilAsync(updateCityCouncilDto);

            _logger.LogInformation($"City council updated successfully with ID: {councilId}");

            return Ok(new UpdateCityCouncilResponse
            {
                Success = true,
                Message = "City council updated successfully",
                CouncilId = updatedCouncil.CouncilId,
                CouncilName = updatedCouncil.Name
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError($"Invalid operation: {ex.Message}");
            return NotFound(new ErrorResponse
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
            _logger.LogError($"Error updating city council: {ex.Message}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while updating the city council"
            });
        }
    }

    /// <summary>
    /// DELETE /api/admin/city-councils/{councilId}
    /// Deletes a city council from the system.
    /// </summary>
    /// <param name="councilId">The ID of the city council to delete</param>
    /// <returns>Success message</returns>
    /// <response code="200">City council deleted successfully</response>
    /// <response code="400">Invalid council ID</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - user does not have admin role</response>
    /// <response code="404">City council not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpDelete("city-councils/{councilId}")]
    [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCityCouncil(Guid councilId)
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

        try
        {
            var deleted = await _cityCouncilService.DeleteCityCouncilAsync(councilId);

            if (!deleted)
            {
                _logger.LogWarning($"City council with ID {councilId} not found for deletion");
                return NotFound(new ErrorResponse
                {
                    Success = false,
                    Message = $"City council with ID {councilId} not found"
                });
            }

            _logger.LogInformation($"City council deleted successfully with ID: {councilId}");

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "City council deleted successfully"
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
            _logger.LogError($"Error deleting city council: {ex.Message}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while deleting the city council"
            });
        }
    }

    #endregion
}
