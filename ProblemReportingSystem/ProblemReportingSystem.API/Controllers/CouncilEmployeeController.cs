using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProblemReportingSystem.API.Contracts.Request;
using ProblemReportingSystem.API.Contracts.Response;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using System.Security.Claims;

namespace ProblemReportingSystem.API.Controllers;

/// <summary>
/// API controller for council employee operations.
/// Manages appeals (problems), polls, and provides statistics/KPI data for council employees.
/// Requires authentication and council employee role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CouncilEmployeeController : ControllerBase
{
    private readonly IAppealService _appealService;
    private readonly IPollService _pollService;
    private readonly IStatisticsService _statisticsService;
    private readonly ICouncilEmployeeService _employeeService;
    private readonly IMapper _mapper;
    private readonly ILogger<CouncilEmployeeController> _logger;

    public CouncilEmployeeController(
        IAppealService appealService,
        IPollService pollService,
        IStatisticsService statisticsService,
        ICouncilEmployeeService employeeService,
        IMapper mapper,
        ILogger<CouncilEmployeeController> logger)
    {
        _appealService = appealService ?? throw new ArgumentNullException(nameof(appealService));
        _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
        _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current user ID from JWT token claims.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user ID in token");
        }
        return userId;
    }

    /// <summary>
    /// Gets the council ID for the current council employee.
    /// </summary>
    private async Task<Guid> GetCurrentCouncilIdAsync()
    {
        var userId = GetCurrentUserId();
        var councilId = await _employeeService.GetCouncilIdByEmployeeIdAsync(userId);
        
        if (councilId == null || councilId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("User is not a council employee or council not found");
        }
        
        return councilId.Value;
    }

    #region Appeals Management

    /// <summary>
    /// GET /api/council-employee/appeals
    /// Gets all appeals (problems) assigned to the current council employee's council.
    /// Returns problems grouped by status for Kanban board display.
    /// </summary>
    /// <returns>List of appeals for the council</returns>
    /// <response code="200">Appeals retrieved successfully</response>
    /// <response code="401">Unauthorized - user is not authenticated</response>
    /// <response code="403">Forbidden - user is not a council employee</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("appeals")]
    [ProducesResponseType(typeof(List<AppealResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAppeals()
    {
        var councilId = await GetCurrentCouncilIdAsync();
        var appeals = await _appealService.GetAppealsByCouncilAsync(councilId);
        var appealResponses = _mapper.Map<List<AppealResponse>>(appeals);
        
        _logger.LogInformation($"Retrieved {appealResponses.Count} appeals for council {councilId}");
        return Ok(appealResponses);
    }

    /// <summary>
    /// PATCH /api/council-employee/appeals/{problemId}/status
    /// Updates the status of a specific problem/appeal.
    /// Valid statuses: Pending, In Progress, Resolved
    /// Also updates the updated_at timestamp via database trigger.
    /// </summary>
    /// <param name="problemId">The ID of the problem to update</param>
    /// <param name="request">The new status</param>
    /// <returns>Updated appeal details</returns>
    /// <response code="200">Status updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Problem not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpPatch("appeals/{problemId}/status")]
    [ProducesResponseType(typeof(AppealResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAppealStatus(
        Guid problemId,
        [FromBody] UpdateAppealStatusRequest request)
    {
        if (problemId == Guid.Empty)
        {
            _logger.LogWarning("Invalid problem ID provided");
            return BadRequest(new { success = false, message = "Invalid problem ID" });
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            _logger.LogWarning("Status is required");
            return BadRequest(new { success = false, message = "Status is required" });
        }

        var validStatuses = new[] { "Pending", "In Progress", "Resolved" };
        if (!validStatuses.Contains(request.Status))
        {
            _logger.LogWarning($"Invalid status: {request.Status}");
            return BadRequest(new { success = false, message = "Invalid status. Valid statuses are: Pending, In Progress, Resolved" });
        }

        await GetCurrentCouncilIdAsync(); // Verify user has permission

        var appeal = await _appealService.UpdateAppealStatusAsync(problemId, request.Status);
        if (appeal == null)
        {
            _logger.LogWarning($"Appeal for problem {problemId} not found");
            return NotFound(new { success = false, message = "Appeal not found" });
        }

        var appealResponse = _mapper.Map<AppealResponse>(appeal);
        _logger.LogInformation($"Appeal status updated to {request.Status} for problem {problemId}");
        return Ok(appealResponse);
    }

    /// <summary>
    /// POST /api/council-employee/appeals/{problemId}/assign
    /// Assigns the current council employee as responsible for this problem/appeal.
    /// </summary>
    /// <param name="problemId">The ID of the problem to assign</param>
    /// <returns>Updated appeal details</returns>
    /// <response code="200">Appeal assigned successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Appeal not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost("appeals/{problemId}/assign")]
    [ProducesResponseType(typeof(AppealResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignAppeal(Guid problemId)
    {
        if (problemId == Guid.Empty)
        {
            _logger.LogWarning("Invalid problem ID provided");
            return BadRequest(new { success = false, message = "Invalid problem ID" });
        }

        var userId = GetCurrentUserId();
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);
        
        if (employee == null)
        {
            _logger.LogWarning($"User {userId} is not a council employee");
            return Forbid();
        }

        var appeal = await _appealService.AssignAppealAsync(problemId, employee.EmployeeId);
        if (appeal == null)
        {
            _logger.LogWarning($"Appeal for problem {problemId} not found");
            return NotFound(new { success = false, message = "Appeal not found" });
        }

        var appealResponse = _mapper.Map<AppealResponse>(appeal);
        _logger.LogInformation($"Problem {problemId} assigned to employee {employee.EmployeeId}");
        return Ok(appealResponse);
    }

    #endregion

    #region Poll Management

    /// <summary>
    /// POST /api/council-employee/polls
    /// Creates a new poll for the current council.
    /// </summary>
    /// <param name="request">Poll creation request with title, description, and options</param>
    /// <returns>Created poll ID</returns>
    /// <response code="201">Poll created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Server error occurred</response>
    [HttpPost("polls")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid poll request model state");
            return BadRequest(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            _logger.LogWarning("Poll title is required");
            return BadRequest(new { success = false, message = "Poll title is required" });
        }

        if (request.Options == null || request.Options.Count < 2)
        {
            _logger.LogWarning("Poll must have at least 2 options");
            return BadRequest(new { success = false, message = "Poll must have at least 2 options" });
        }

        var councilId = await GetCurrentCouncilIdAsync();

        var createPollDto = new CreatePollDto
        {
            CouncilId = councilId,
            Title = request.Title,
            Description = request.Description
        };

        var pollId = await _pollService.CreatePollAsync(createPollDto);

        _logger.LogInformation($"Poll created successfully with ID: {pollId}");
        return CreatedAtAction(nameof(CreatePoll), new { id = pollId }, new { id = pollId });
    }

    /// <summary>
    /// GET /api/council-employee/polls
    /// Gets all polls created by the current council.
    /// Shows poll status (active/closed) and vote results.
    /// </summary>
    /// <returns>List of polls for the council</returns>
    /// <response code="200">Polls retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("polls")]
    [ProducesResponseType(typeof(List<PollResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCouncilPolls()
    {
        var councilId = await GetCurrentCouncilIdAsync();
        var polls = await _pollService.GetCouncilPollsAsync(councilId);
        
        var pollResponses = polls.Select(p => new PollResponse
        {
            PollId = p.PollId,
            Title = p.Title,
            Description = p.Description,
            PublishedAt = p.PublishedAt,
            IsActive = p.IsActive,
            Options = p.PollOptions.Select(opt => new PollOptionResponse
            {
                OptionId = opt.OptionId,
                OptionText = opt.OptionText,
                VoteCount = opt.VoteCount,
                VotePercentage = opt.VoteCount > 0 ? (decimal)opt.VoteCount / p.PollOptions.Sum(x => x.VoteCount) * 100 : 0
            }).ToList()
        }).ToList();

        _logger.LogInformation($"Retrieved {pollResponses.Count} polls for council {councilId}");
        return Ok(pollResponses);
    }

    /// <summary>
    /// PATCH /api/council-employee/polls/{pollId}/close
    /// Deactivates a poll (is_active = false).
    /// Citizens will no longer be able to vote on this poll.
    /// </summary>
    /// <param name="pollId">The ID of the poll to close</param>
    /// <returns>Success message</returns>
    /// <response code="200">Poll closed successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Poll not found</response>
    /// <response code="500">Server error occurred</response>
    [HttpPatch("polls/{pollId}/close")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClosePoll(Guid pollId)
    {
        if (pollId == Guid.Empty)
        {
            _logger.LogWarning("Invalid poll ID provided");
            return BadRequest(new { success = false, message = "Invalid poll ID" });
        }

        await GetCurrentCouncilIdAsync(); // Verify permission

        var result = await _pollService.ClosePollAsync(pollId);
        if (!result)
        {
            _logger.LogWarning($"Poll {pollId} not found");
            return NotFound(new { success = false, message = "Poll not found" });
        }

        _logger.LogInformation($"Poll {pollId} closed successfully");
        return Ok(new { success = true, message = "Poll closed successfully" });
    }

    #endregion

    #region Statistics & KPI

    /// <summary>
    /// GET /api/council-employee/statistics/kpi
    /// Returns KPI data for dashboard widgets:
    /// - Total Appeals
    /// - Pending (Needs attention)
    /// - Resolved
    /// - Avg Resolution (average time in days)
    /// </summary>
    /// <returns>KPI statistics</returns>
    /// <response code="200">KPI retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("statistics/kpi")]
    [ProducesResponseType(typeof(KPIResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetKPI()
    {
        var councilId = await GetCurrentCouncilIdAsync();
        var statistics = await _statisticsService.GetCouncilKPIAsync(councilId);

        var kpiResponse = new KPIResponse
        {
            TotalAppeals = statistics.TotalAppeals,
            PendingAppeals = statistics.PendingAppeals,
            ResolvedAppeals = statistics.ResolvedAppeals,
            AverageResolutionDays = statistics.AverageResolutionDays
        };

        _logger.LogInformation($"Retrieved KPI for council {councilId}");
        return Ok(kpiResponse);
    }

    /// <summary>
    /// GET /api/council-employee/statistics/efficiency
    /// Returns efficiency report data for the dashboard.
    /// Includes total appeals, resolved count, efficiency percentage, and daily breakdown.
    /// </summary>
    /// <returns>Efficiency report</returns>
    /// <response code="200">Efficiency data retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="500">Server error occurred</response>
    [HttpGet("statistics/efficiency")]
    [ProducesResponseType(typeof(EfficiencyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEfficiency()
    {
        var councilId = await GetCurrentCouncilIdAsync();
        var efficiency = await _statisticsService.GetCouncilEfficiencyAsync(councilId);

        if (efficiency == null)
        {
            _logger.LogWarning($"Efficiency report not found for council {councilId}");
            return NotFound(new { success = false, message = "Efficiency report not found" });
        }

        var efficiencyResponse = new EfficiencyResponse
        {
            CouncilId = efficiency.CouncilId,
            CouncilName = efficiency.CouncilName,
            TotalAppeals = efficiency.TotalAppeals,
            ResolvedAppeals = efficiency.ResolvedAppeals,
            EfficiencyPercentage = efficiency.EfficiencyPercentage,
            DailyData = new List<DailyEfficiencyPoint>()
        };

        _logger.LogInformation($"Retrieved efficiency report for council {councilId}");
        return Ok(efficiencyResponse);
    }

    #endregion
}

