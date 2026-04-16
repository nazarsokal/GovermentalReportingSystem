using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProblemReportingSystem.API.Contracts.Request;
using ProblemReportingSystem.API.Contracts.Response;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;

namespace ProblemReportingSystem.API.Controllers;

/// <summary>
/// API controller for problem management operations.
/// Provides endpoints for creating, updating, and retrieving problems reported by users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProblemController : ControllerBase
{
    private readonly IProblemService _problemService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProblemController> _logger;

    public ProblemController(IProblemService problemService, IMapper mapper, ILogger<ProblemController> logger)
    {
        _problemService = problemService ?? throw new ArgumentNullException(nameof(problemService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProblem([FromForm] CreateProblemRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid request model state for CreateProblem");
            return BadRequest(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        // 1. Мапимо базові текстові поля (назва, опис, координати) з Request у DTO
        var createProblemDto = _mapper.Map<CreateProblemDto>(request);
        
        // Ініціалізуємо колекцію фотографій, якщо вона ще не створена
        createProblemDto.Photos = new List<CreateProblemPhotoDto>();

        if (request.Photos != null && request.Photos.Any())
        {
            foreach (var file in request.Photos)
            {
                if (file.Length > 0)
                {
                    using var memoryStream = new MemoryStream();

                    await file.CopyToAsync(memoryStream, cancellationToken);
                    
                    createProblemDto.Photos.Add(new CreateProblemPhotoDto
                    {
                        ImageData = memoryStream.ToArray(),
                        ContentType = file.ContentType 
                    });
                }
            }
        }
        
        // 3. Передаємо DTO з уже підготовленими байтами у сервіс
        var problemId = await _problemService.CreateProblem(createProblemDto);

        _logger.LogInformation($"Problem created successfully with ID: {problemId}");
        return CreatedAtAction(nameof(CreateProblem), new { id = problemId }, new { id = problemId });
    }

    /// <summary>
    /// Retrieves a problem by its ID with all related details.
    /// </summary>
    /// <param name="id">The ID of the problem to retrieve</param>
    /// <returns>Problem details including address, category, and photos</returns>
    /// <response code="200">Problem retrieved successfully</response>
    /// <response code="404">Problem not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DetailProblemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProblemById(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid problem ID provided");
            return BadRequest(new { success = false, message = "Invalid problem ID" });
        }

        var problem = await _problemService.GetProblemByIdAsync(id);

        if (problem == null)
        {
            _logger.LogWarning($"Problem with ID {id} not found");
            return NotFound(new { success = false, message = "Problem not found" });
        }

        var detailResponse = _mapper.Map<DetailProblemResponse>(problem);
        return Ok(detailResponse);
    }

    /// <summary>
    /// Retrieves all problems with summary information.
    /// </summary>
    /// <returns>List of all problems with summary details</returns>
    /// <response code="200">Problems retrieved successfully</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SummaryProblemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllProblems()
    {
        _logger.LogInformation("Retrieving all problems");
        
        var problems = await _problemService.GetAllProblems();
        
        var summaryResponses = _mapper.Map<IEnumerable<SummaryProblemResponse>>(problems);
        
        _logger.LogInformation($"Retrieved {summaryResponses.Count()} problems");
        return Ok(summaryResponses);
    }

    /// <summary>
    /// Updates an existing problem with new information.
    /// </summary>
    /// <param name="request">The update problem request containing problem ID and new details</param>
    /// <returns>The ID of the updated problem</returns>
    /// <response code="200">Problem updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Problem not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpPut]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProblem([FromBody] UpdateProblemRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid request model state for UpdateProblem");
            return BadRequest(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        var updateProblemDto = _mapper.Map<UpdateProblemDto>(request);
        var problemId = await _problemService.UpdateProblem(updateProblemDto);

        _logger.LogInformation($"Problem with ID {problemId} updated successfully");
        return Ok(new { id = problemId });
    }

    /// <summary>
    /// Retrieves all problems reported by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>List of problems reported by the user</returns>
    /// <response code="200">Problems retrieved successfully</response>
    /// <response code="400">Invalid user ID</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<SummaryProblemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserProblems(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user ID provided");
            return BadRequest(new { success = false, message = "Invalid user ID" });
        }

        _logger.LogInformation($"Retrieving problems for user {userId}");
        var problems = await _problemService.GetUserProblems(userId);
        var summaryResponses = _mapper.Map<IEnumerable<SummaryProblemResponse>>(problems);

        _logger.LogInformation($"Retrieved {summaryResponses.Count()} problems for user {userId}");
        return Ok(summaryResponses);
    }

    /// <summary>
    /// Retrieves all problems with a specific status.
    /// </summary>
    /// <param name="status">The status to filter by</param>
    /// <returns>List of problems with the specified status</returns>
    /// <response code="200">Problems retrieved successfully</response>
    /// <response code="400">Invalid status</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<SummaryProblemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProblemsByStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            _logger.LogWarning("Invalid status provided");
            return BadRequest(new { success = false, message = "Status cannot be empty" });
        }

        _logger.LogInformation($"Retrieving problems with status: {status}");
        var problems = await _problemService.GetProblemsByStatus(status);
        var summaryResponses = _mapper.Map<IEnumerable<SummaryProblemResponse>>(problems);

        _logger.LogInformation($"Retrieved {summaryResponses.Count()} problems with status {status}");
        return Ok(summaryResponses);
    }

    /// <summary>
    /// Retrieves all problems in a specific category.
    /// </summary>
    /// <param name="categoryId">The ID of the category</param>
    /// <returns>List of problems in the specified category</returns>
    /// <response code="200">Problems retrieved successfully</response>
    /// <response code="400">Invalid category ID</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(typeof(IEnumerable<SummaryProblemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProblemsByCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            _logger.LogWarning("Invalid category ID provided");
            return BadRequest(new { success = false, message = "Invalid category ID" });
        }

        _logger.LogInformation($"Retrieving problems for category {categoryId}");
        var problems = await _problemService.GetProblemsByCategory(categoryId);
        var summaryResponses = _mapper.Map<IEnumerable<SummaryProblemResponse>>(problems);

        _logger.LogInformation($"Retrieved {summaryResponses.Count()} problems for category {categoryId}");
        return Ok(summaryResponses);
    }

    /// <summary>
    /// Retrieves problems within geographic bounds (useful for map-based queries).
    /// </summary>
    /// <param name="request">The geographic bounds request containing latitude and longitude ranges</param>
    /// <returns>List of problems within the specified geographic bounds</returns>
    /// <response code="200">Problems retrieved successfully</response>
    /// <response code="400">Invalid geographic bounds</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost("bounds")]
    [ProducesResponseType(typeof(IEnumerable<SummaryProblemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProblemsInBounds([FromBody] GetProblemsInBoundsRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid request model state for GetProblemsInBounds");
            return BadRequest(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        if (request.MinLatitude > request.MaxLatitude || request.MinLongitude > request.MaxLongitude)
        {
            _logger.LogWarning("Invalid geographic bounds provided");
            return BadRequest(new { success = false, message = "Minimum values cannot be greater than maximum values" });
        }

        _logger.LogInformation($"Retrieving problems within bounds: Lat({request.MinLatitude:F6}-{request.MaxLatitude:F6}), Lng({request.MinLongitude:F6}-{request.MaxLongitude:F6})");
        var problems = await _problemService.GetProblemsInBounds(request.MinLatitude, request.MaxLatitude, request.MinLongitude, request.MaxLongitude);
        var summaryResponses = _mapper.Map<IEnumerable<SummaryProblemResponse>>(problems);

        _logger.LogInformation($"Retrieved {summaryResponses.Count()} problems within geographic bounds");
        return Ok(summaryResponses);
    }

    /// <summary>
    /// Retrieves all problems assigned to a specific council employee.
    /// </summary>
    /// <param name="employeeId">The ID of the council employee</param>
    /// <returns>List of problems assigned to the employee</returns>
    /// <response code="200">Problems retrieved successfully</response>
    /// <response code="400">Invalid employee ID</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("assigned/{employeeId}")]
    [ProducesResponseType(typeof(IEnumerable<SummaryProblemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssignedProblems(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
        {
            _logger.LogWarning("Invalid employee ID provided");
            return BadRequest(new { success = false, message = "Invalid employee ID" });
        }

        _logger.LogInformation($"Retrieving problems assigned to employee {employeeId}");
        var problems = await _problemService.GetAssignedProblems(employeeId);
        var summaryResponses = _mapper.Map<IEnumerable<SummaryProblemResponse>>(problems);

        _logger.LogInformation($"Retrieved {summaryResponses.Count()} problems assigned to employee {employeeId}");
        return Ok(summaryResponses);
    }
}