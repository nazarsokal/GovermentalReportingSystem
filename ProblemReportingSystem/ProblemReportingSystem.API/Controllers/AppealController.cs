using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ProblemReportingSystem.API.Contracts.Request;
using ProblemReportingSystem.API.Contracts.Response;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;

namespace ProblemReportingSystem.API.Controllers;

/// <summary>
/// API controller for appeal management operations.
/// Handles creation, retrieval, update, deletion of appeals and provides statistics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AppealController : ControllerBase
{
    private readonly IAppealService _appealService;
    private readonly IProblemCategoryService _categoryService;
    private readonly IMapper _mapper;
    private readonly ILogger<AppealController> _logger;

    public AppealController(
        IAppealService appealService,
        IProblemCategoryService categoryService,
        IMapper mapper,
        ILogger<AppealController> logger)
    {
        _appealService = appealService ?? throw new ArgumentNullException(nameof(appealService));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ===== CREATE =====
    /// <summary>
    /// Creates a new appeal with problem details.
    /// </summary>
    /// <param name="request">The appeal creation request</param>
    /// <returns>The ID of the created appeal</returns>
    /// <response code="201">Appeal created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAppeal([FromForm] CreateAppealRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid request model state for CreateAppeal");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
            });
        }

        try
        {
            var appealDto = _mapper.Map<AppealDto>(request);
            
            // Extract UserId from JWT claims if not provided
            if (appealDto.UserId == Guid.Empty)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    appealDto.UserId = userId;
                }
                else
                {
                    _logger.LogWarning("Could not extract valid UserId from authentication claims");
                    return BadRequest(new ErrorResponse
                    {
                        Success = false,
                        Message = "Invalid user context"
                    });
                }
            }

            // Handle file uploads for photos
            if (request.Photos != null && request.Photos.Any())
            {
                appealDto.ProblemDto.Photos = new List<CreateProblemPhotoDto>();
                foreach (var file in request.Photos)
                {
                    if (file.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream, cancellationToken);

                        appealDto.ProblemDto.Photos.Add(new CreateProblemPhotoDto
                        {
                            ImageData = memoryStream.ToArray(),
                            ContentType = file.ContentType
                        });
                    }
                }
            }

            var appealId = await _appealService.CreateAppealAsync(appealDto);

            _logger.LogInformation($"Appeal created successfully with ID: {appealId}");
            return CreatedAtAction(nameof(GetAppealById), new { id = appealId }, new { id = appealId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appeal");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while creating the appeal"
            });
        }
    }

    // ===== READ =====
    /// <summary>
    /// Retrieves an appeal by its ID with all details.
    /// </summary>
    /// <param name="id">The appeal ID</param>
    /// <returns>Appeal details</returns>
    /// <response code="200">Appeal retrieved successfully</response>
    /// <response code="404">Appeal not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppealDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAppealById(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid appeal ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid appeal ID"
            });
        }

        try
        {
            var appeal = await _appealService.GetAppealByIdAsync(id);
            if (appeal == null)
            {
                _logger.LogWarning($"Appeal with ID {id} not found");
                return NotFound(new ErrorResponse
                {
                    Success = false,
                    Message = "Appeal not found"
                });
            }

            return Ok(appeal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving appeal {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the appeal"
            });
        }
    }

    /// <summary>
    /// Retrieves all appeals.
    /// </summary>
    /// <returns>List of all appeals</returns>
    /// <response code="200">Appeals retrieved successfully</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AppealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAppeals()
    {
        try
        {
            _logger.LogInformation("Retrieving all appeals");
            var appeals = await _appealService.GetAllAppealsAsync();
            return Ok(appeals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all appeals");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving appeals"
            });
        }
    }

    /// <summary>
    /// Retrieves appeals for a specific user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>List of user's appeals</returns>
    /// <response code="200">Appeals retrieved successfully</response>
    /// <response code="400">Invalid user ID</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<AppealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserAppeals(Guid userId)
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
            _logger.LogInformation($"Retrieving appeals for user {userId}");
            var appeals = await _appealService.GetUserAppealsAsync(userId);
            return Ok(appeals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving appeals for user {userId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving user appeals"
            });
        }
    }

    /// <summary>
    /// Retrieves appeals assigned to a specific council employee.
    /// </summary>
    /// <param name="employeeId">The employee ID</param>
    /// <returns>List of appeals assigned to the employee</returns>
    /// <response code="200">Appeals retrieved successfully</response>
    /// <response code="400">Invalid employee ID</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("employee/{employeeId}")]
    [ProducesResponseType(typeof(IEnumerable<AppealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEmployeeAppeals(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
        {
            _logger.LogWarning("Invalid employee ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid employee ID"
            });
        }

        try
        {
            _logger.LogInformation($"Retrieving appeals for employee {employeeId}");
            var appeals = await _appealService.GetCouncilAppealsByEmployeeAsync(employeeId);
            return Ok(appeals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving appeals for employee {employeeId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving employee appeals"
            });
        }
    }

    /// <summary>
    /// Retrieves appeals for a specific council.
    /// </summary>
    /// <param name="councilId">The council ID</param>
    /// <returns>List of council's appeals</returns>
    /// <response code="200">Appeals retrieved successfully</response>
    /// <response code="400">Invalid council ID</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("council/{councilId}")]
    [ProducesResponseType(typeof(IEnumerable<AppealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCouncilAppeals(Guid councilId)
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
            _logger.LogInformation($"Retrieving appeals for council {councilId}");
            var appeals = await _appealService.GetAppealsByCouncilAsync(councilId);
            return Ok(appeals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving appeals for council {councilId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving council appeals"
            });
        }
    }

    /// <summary>
    /// Retrieves appeals with a specific status in a council.
    /// </summary>
    /// <param name="councilId">The council ID</param>
    /// <param name="status">The appeal status (e.g., Pending, Resolved, In Progress)</param>
    /// <returns>List of appeals with the specified status</returns>
    /// <response code="200">Appeals retrieved successfully</response>
    /// <response code="400">Invalid parameters</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("council/{councilId}/status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<AppealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAppealsByStatus(Guid councilId, string status)
    {
        if (councilId == Guid.Empty || string.IsNullOrWhiteSpace(status))
        {
            _logger.LogWarning("Invalid council ID or status provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid council ID or status"
            });
        }

        try
        {
            _logger.LogInformation($"Retrieving appeals for council {councilId} with status {status}");
            var appeals = await _appealService.GetAppealsByStatusAsync(councilId, status);
            return Ok(appeals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving appeals for council {councilId} with status {status}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving appeals"
            });
        }
    }

    /// <summary>
    /// Retrieves unassigned appeals.
    /// </summary>
    /// <returns>List of unassigned appeals</returns>
    /// <response code="200">Appeals retrieved successfully</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("unassigned")]
    [ProducesResponseType(typeof(IEnumerable<AppealDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUnassignedAppeals()
    {
        try
        {
            _logger.LogInformation("Retrieving unassigned appeals");
            var appeals = await _appealService.GetUnassignedAppealsAsync();
            return Ok(appeals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unassigned appeals");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving unassigned appeals"
            });
        }
    }

    /// <summary>
    /// Retrieves all appeals formatted for map display.
    /// Returns minimal information with coordinates for mapping purposes.
    /// </summary>
    /// <returns>List of appeals with map information</returns>
    /// <response code="200">Appeals retrieved successfully</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("map/{district}")]
    [ProducesResponseType(typeof(IEnumerable<SummaryAppealForMapResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAppealsForMap(string district)
    {
        try
        {
            _logger.LogInformation("Retrieving appeals for map display");
            var appeals = await _appealService.GetAppealsByDistrictAsync(district);
            var mappedAppeals = _mapper.Map<IEnumerable<SummaryAppealForMapResponse>>(appeals).ToList();
            
            // Populate category icon URLs from database
            foreach (var appeal in mappedAppeals)
            {
                // The CategoryIconUrl currently contains the CategoryId from the mapping
                if (Guid.TryParse(appeal.CategoryIconUrl, out var categoryId))
                {
                    try
                    {
                        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                        appeal.CategoryIconUrl = category?.IconUrl ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to fetch category icon for {categoryId}: {ex.Message}");
                        appeal.CategoryIconUrl = string.Empty;
                    }
                }
            }
            
            return Ok(mappedAppeals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appeals for map");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving appeals for map display"
            });
        }
    }

    /// <summary>
    /// Helper method to get category icon URL.
    /// In a real implementation, this should fetch from database through a service.
    /// </summary>
    private string GetCategoryIconUrl(Guid categoryId)
    {
        // This is a placeholder - ideally this should be injected as a service
        // that queries the database for the actual icon URL
        // For now, we'll use a mapping based on common problem categories
        var categoryIconMap = new Dictionary<string, string>
        {
            { "pothole", "https://api.example.com/icons/pothole.png" },
            { "street-light", "https://api.example.com/icons/street-light.png" },
            { "trash", "https://api.example.com/icons/trash.png" },
            { "sidewalk", "https://api.example.com/icons/sidewalk.png" },
            { "water-leak", "https://api.example.com/icons/water-leak.png" },
            { "default", "https://api.example.com/icons/default.png" }
        };
        
        return categoryIconMap["default"];
    }

    // ===== UPDATE =====
    /// <summary>
    /// Updates the status of an appeal.
    /// </summary>
    /// <param name="id">The appeal ID</param>
    /// <param name="request">The status update request</param>
    /// <returns>Updated appeal</returns>
    /// <response code="200">Appeal status updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Appeal not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(AppealDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAppealStatus(Guid id, [FromBody] UpdateAppealStatusRequest request)
    {
        if (id == Guid.Empty || request == null || string.IsNullOrWhiteSpace(request.Status))
        {
            _logger.LogWarning("Invalid appeal ID or status provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid appeal ID or status"
            });
        }

        try
        {
            _logger.LogInformation($"Updating status for appeal {id} to {request.Status}");
            var result = await _appealService.UpdateAppealStatusAsync(id, request.Status);
            
            if (result == null)
            {
                return NotFound(new ErrorResponse
                {
                    Success = false,
                    Message = "Appeal not found"
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating appeal status for {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while updating the appeal status"
            });
        }
    }

    /// <summary>
    /// Assigns an appeal to a council employee.
    /// </summary>
    /// <param name="id">The appeal ID</param>
    /// <param name="request">The assignment request</param>
    /// <returns>Updated appeal</returns>
    /// <response code="200">Appeal assigned successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Appeal not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpPut("{id}/assign")]
    [ProducesResponseType(typeof(AppealDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignAppeal(Guid id, [FromBody] AssignAppealRequest request)
    {
        if (id == Guid.Empty || request == null || request.EmployeeId == Guid.Empty)
        {
            _logger.LogWarning("Invalid appeal ID or employee ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid appeal ID or employee ID"
            });
        }

        try
        {
            _logger.LogInformation($"Assigning appeal {id} to employee {request.EmployeeId}");
            var result = await _appealService.AssignAppealAsync(id, request.EmployeeId);
            
            if (result == null)
            {
                return NotFound(new ErrorResponse
                {
                    Success = false,
                    Message = "Appeal not found"
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error assigning appeal {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while assigning the appeal"
            });
        }
    }

    /// <summary>
    /// Unassigns an appeal from its assigned employee.
    /// </summary>
    /// <param name="id">The appeal ID</param>
    /// <returns>Updated appeal</returns>
    /// <response code="200">Appeal unassigned successfully</response>
    /// <response code="404">Appeal not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpPut("{id}/unassign")]
    [ProducesResponseType(typeof(AppealDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnassignAppeal(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid appeal ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid appeal ID"
            });
        }

        try
        {
            _logger.LogInformation($"Unassigning appeal {id}");
            var result = await _appealService.UnassignAppealAsync(id);
            
            if (result == null)
            {
                return NotFound(new ErrorResponse
                {
                    Success = false,
                    Message = "Appeal not found"
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error unassigning appeal {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while unassigning the appeal"
            });
        }
    }

    // ===== DELETE =====
    /// <summary>
    /// Deletes an appeal.
    /// </summary>
    /// <param name="id">The appeal ID</param>
    /// <returns>Deletion confirmation</returns>
    /// <response code="200">Appeal deleted successfully</response>
    /// <response code="400">Invalid appeal ID</response>
    /// <response code="404">Appeal not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAppeal(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid appeal ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid appeal ID"
            });
        }

        try
        {
            _logger.LogInformation($"Deleting appeal {id}");
            var result = await _appealService.DeleteAppealAsync(id);
            
            if (!result)
            {
                return NotFound(new ErrorResponse
                {
                    Success = false,
                    Message = "Appeal not found"
                });
            }

            return Ok(new { success = true, message = "Appeal deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting appeal {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while deleting the appeal"
            });
        }
    }

    // ===== STATISTICS =====
    /// <summary>
    /// Retrieves overall appeal statistics.
    /// </summary>
    /// <returns>Appeal statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("statistics/overall")]
    [ProducesResponseType(typeof(AppealStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOverallStatistics()
    {
        try
        {
            _logger.LogInformation("Retrieving overall appeal statistics");
            var stats = await _appealService.GetAppealStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overall statistics");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving statistics"
            });
        }
    }

    /// <summary>
    /// Retrieves statistics for a specific council.
    /// </summary>
    /// <param name="councilId">The council ID</param>
    /// <returns>Council appeal statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="400">Invalid council ID</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("statistics/council/{councilId}")]
    [ProducesResponseType(typeof(AppealStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCouncilStatistics(Guid councilId)
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
            _logger.LogInformation($"Retrieving statistics for council {councilId}");
            var stats = await _appealService.GetCouncilAppealStatisticsAsync(councilId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving statistics for council {councilId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving council statistics"
            });
        }
    }

    /// <summary>
    /// Retrieves statistics for a specific employee.
    /// </summary>
    /// <param name="employeeId">The employee ID</param>
    /// <returns>Employee appeal statistics</returns>
    /// <response code="200">Statistics retrieved successfully</response>
    /// <response code="400">Invalid employee ID</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("statistics/employee/{employeeId}")]
    [ProducesResponseType(typeof(AppealStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEmployeeStatistics(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
        {
            _logger.LogWarning("Invalid employee ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid employee ID"
            });
        }

        try
        {
            _logger.LogInformation($"Retrieving statistics for employee {employeeId}");
            var stats = await _appealService.GetEmployeeAppealStatisticsAsync(employeeId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving statistics for employee {employeeId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving employee statistics"
            });
        }
    }

    /// <summary>
    /// Retrieves overall status distribution of appeals.
    /// </summary>
    /// <returns>Status distribution dictionary</returns>
    /// <response code="200">Distribution retrieved successfully</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("statistics/status-distribution")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatusDistribution()
    {
        try
        {
            _logger.LogInformation("Retrieving overall status distribution");
            var distribution = await _appealService.GetAppealStatusDistributionAsync();
            return Ok(distribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving status distribution");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving status distribution"
            });
        }
    }

    /// <summary>
    /// Retrieves status distribution for a specific council.
    /// </summary>
    /// <param name="councilId">The council ID</param>
    /// <returns>Status distribution dictionary</returns>
    /// <response code="200">Distribution retrieved successfully</response>
    /// <response code="400">Invalid council ID</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("statistics/council/{councilId}/status-distribution")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCouncilStatusDistribution(Guid councilId)
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
            _logger.LogInformation($"Retrieving status distribution for council {councilId}");
            var distribution = await _appealService.GetCouncilStatusDistributionAsync(councilId);
            return Ok(distribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving status distribution for council {councilId}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving council status distribution"
            });
        }
    }

    /// <summary>
    /// Retrieves the average resolution time in days.
    /// </summary>
    /// <returns>Average resolution time</returns>
    /// <response code="200">Average retrieved successfully</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("statistics/average-resolution-time")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAverageResolutionTime()
    {
        try
        {
            _logger.LogInformation("Retrieving average resolution time");
            var avgDays = await _appealService.GetAverageResolutionTimeInDaysAsync();
            return Ok(new { averageResolutionDays = avgDays });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving average resolution time");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving average resolution time"
            });
        }
    }

    /// <summary>
    /// Retrieves appeal trends for the last N days.
    /// </summary>
    /// <param name="daysBack">Number of days to look back (default: 30)</param>
    /// <returns>Appeal trend data</returns>
    /// <response code="200">Trend data retrieved successfully</response>
    /// <response code="400">Invalid days back value</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("statistics/trends")]
    [ProducesResponseType(typeof(AppealTrendDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTrends([FromQuery] int daysBack = 30)
    {
        if (daysBack <= 0)
        {
            _logger.LogWarning("Invalid daysBack value provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Days back must be greater than 0"
            });
        }

        try
        {
            _logger.LogInformation($"Retrieving appeal trends for the last {daysBack} days");
            var trends = await _appealService.GetAppealTrendAsync(daysBack);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving appeal trends");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving appeal trends"
            });
        }
    }

    /// <summary>
    /// Retrieves a summary of an appeal with essential information.
    /// </summary>
    /// <param name="id">The appeal ID</param>
    /// <returns>Appeal summary with Id, DatePublished, ProblemName, Status, and Description</returns>
    /// <response code="200">Appeal summary retrieved successfully</response>
    /// <response code="400">Invalid appeal ID</response>
    /// <response code="404">Appeal not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("{id}/summary")]
    [ProducesResponseType(typeof(AppealSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAppealSummary(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid appeal ID provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Invalid appeal ID"
            });
        }

        try
        {
            var appeal = await _appealService.GetAppealByIdAsync(id);
            if (appeal == null)
            {
                _logger.LogWarning($"Appeal with ID {id} not found");
                return NotFound(new ErrorResponse
                {
                    Success = false,
                    Message = "Appeal not found"
                });
            }

            var summary = _mapper.Map<AppealSummaryResponse>(appeal);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving appeal summary for {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the appeal summary"
            });
        }
    }

    /// <summary>
    /// Retrieves a summary of appeals for a specific city.
    /// </summary>
    /// <param name="city">The city name</param>
    /// <returns>List of appeal summaries for the city</returns>
    /// <response code="200">Appeals retrieved successfully</response>
    /// <response code="400">Invalid city parameter</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("summary/city/{city}")]
    [ProducesResponseType(typeof(IEnumerable<AppealSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAppealSummariesByCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            _logger.LogWarning("Invalid city parameter provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "City cannot be empty"
            });
        }

        try
        {
            var appeals = await _appealService.GetAppealsByCityAsync(city);
            var summaries = _mapper.Map<IEnumerable<AppealSummaryResponse>>(appeals);
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving appeal summaries for city {city}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving appeal summaries"
            });
        }
    }

    /// <summary>
    /// Retrieves a summary of appeals for a specific oblast.
    /// </summary>
    /// <param name="oblast">The oblast name</param>
    /// <returns>List of appeal summaries for the oblast</returns>
    /// <response code="200">Appeals retrieved successfully</response>
    /// <response code="400">Invalid oblast parameter</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("summary/oblast/{oblast}")]
    [ProducesResponseType(typeof(IEnumerable<AppealSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAppealSummariesByOblast(string oblast)
    {
        if (string.IsNullOrWhiteSpace(oblast))
        {
            _logger.LogWarning("Invalid oblast parameter provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "Oblast cannot be empty"
            });
        }

        try
        {
            var appeals = await _appealService.GetAppealsByOblastAsync(oblast);
            var summaries = _mapper.Map<IEnumerable<AppealSummaryResponse>>(appeals);
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving appeal summaries for oblast {oblast}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving appeal summaries"
            });
        }
    }

    /// <summary>
    /// Retrieves a summary of appeals for a specific district.
    /// </summary>
    /// <param name="district">The district name</param>
    /// <returns>List of appeal summaries for the district</returns>
    /// <response code="200">Appeals retrieved successfully</response>
    /// <response code="400">Invalid district parameter</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("summary/district/{district}")]
    [ProducesResponseType(typeof(IEnumerable<AppealSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAppealSummariesByDistrict(string district)
    {
        if (string.IsNullOrWhiteSpace(district))
        {
            _logger.LogWarning("Invalid district parameter provided");
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Message = "District cannot be empty"
            });
        }

        try
        {
            var appeals = await _appealService.GetAppealsByDistrictAsync(district);
            var summaries = _mapper.Map<IEnumerable<AppealSummaryResponse>>(appeals);
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving appeal summaries for district {district}");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Success = false,
                Message = "An error occurred while retrieving appeal summaries"
            });
        }
    }
}
