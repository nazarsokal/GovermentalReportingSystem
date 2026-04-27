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
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);
        var councilId = employee?.CouncilId;
        
        if (councilId == null || councilId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("User is not a council employee or council not found");
        }
        
        return councilId.Value;
    }

    private async Task<bool> IsCurrentUserCouncilHeadAsync()
    {
        var userId = GetCurrentUserId();
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);
        return employee != null &&
               !string.IsNullOrWhiteSpace(employee.Position) &&
               employee.Position.Equals("Head", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<CouncilEmployeeDto?> GetCurrentEmployeeAsync()
    {
        var userId = GetCurrentUserId();
        return await _employeeService.GetEmployeeByUserIdAsync(userId);
    }

    private static bool IsValidStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status)) return false;
        return status is "Pending" or "In Progress" or "Resolved";
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
    /// GET /api/council-employee/appeals/council
    /// Gets all appeals assigned to the current employee's council.
    /// </summary>
    [HttpGet("appeals/council")]
    [ProducesResponseType(typeof(List<AppealResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCouncilAppeals()
    {
        var councilId = await GetCurrentCouncilIdAsync();
        var appeals = await _appealService.GetAppealsByCouncilAsync(councilId);
        return Ok(_mapper.Map<List<AppealResponse>>(appeals));
    }

    /// <summary>
    /// GET /api/council-employee/appeals/employee
    /// Gets appeals assigned to current council employee.
    /// </summary>
    [HttpGet("appeals/employee")]
    [ProducesResponseType(typeof(List<AppealResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentEmployeeAppeals()
    {
        var currentUserId = GetCurrentUserId();
        var employee = await _employeeService.GetEmployeeByUserIdAsync(currentUserId);
        if (employee == null)
        {
            return Forbid();
        }

        var appeals = await _appealService.GetCouncilAppealsByEmployeeAsync(employee.EmployeeId);
        return Ok(_mapper.Map<List<AppealResponse>>(appeals));
    }

    /// <summary>
    /// GET /api/council-employee/appeals/status/{status}
    /// Gets appeals by status for current employee's council.
    /// </summary>
    [HttpGet("appeals/status/{status}")]
    [ProducesResponseType(typeof(List<AppealResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAppealsByStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return BadRequest(new { success = false, message = "Status is required" });
        }

        var councilId = await GetCurrentCouncilIdAsync();
        var appeals = await _appealService.GetAppealsByStatusAsync(councilId, status);
        return Ok(_mapper.Map<List<AppealResponse>>(appeals));
    }

    /// <summary>
    /// GET /api/council-employee/appeals/unassigned
    /// Gets unassigned appeals from current employee's council.
    /// </summary>
    [HttpGet("appeals/unassigned")]
    [ProducesResponseType(typeof(List<AppealResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCouncilUnassignedAppeals()
    {
        var councilId = await GetCurrentCouncilIdAsync();
        var councilAppeals = await _appealService.GetAppealsByCouncilAsync(councilId);
        var unassigned = councilAppeals.Where(a => !a.AssignedEmployeeId.HasValue).ToList();
        return Ok(_mapper.Map<List<AppealResponse>>(unassigned));
    }

    /// <summary>
    /// GET /api/council-employee/appeals/map/{district}
    /// Gets district appeals for map view.
    /// </summary>
    [HttpGet("appeals/map/{district}")]
    [ProducesResponseType(typeof(List<SummaryAppealForMapResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAppealsForMap(string district)
    {
        if (string.IsNullOrWhiteSpace(district))
        {
            return BadRequest(new { success = false, message = "District is required" });
        }

        var appeals = await _appealService.GetAppealsByDistrictAsync(district);
        return Ok(_mapper.Map<List<SummaryAppealForMapResponse>>(appeals));
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

        if (!IsValidStatus(request.Status))
        {
            _logger.LogWarning($"Invalid status: {request.Status}");
            return BadRequest(new { success = false, message = "Invalid status. Valid statuses are: Pending, In Progress, Resolved" });
        }

        var councilId = await GetCurrentCouncilIdAsync(); // Verify user has permission (council employee)

        // AuthZ:
        // - Head can change status for any appeal in their council
        // - Regular employee can change status only for appeals assigned to them
        var isHead = await IsCurrentUserCouncilHeadAsync();
        if (!isHead)
        {
            var currentEmployee = await GetCurrentEmployeeAsync();
            if (currentEmployee == null)
            {
                return Forbid();
            }

            var councilAppeals = await _appealService.GetAppealsByCouncilAsync(councilId);
            var target = councilAppeals.FirstOrDefault(a => a.AppealId == problemId);
            if (target == null)
            {
                return NotFound(new { success = false, message = "Appeal not found" });
            }

            if (target.AssignedEmployeeId != currentEmployee.EmployeeId)
            {
                return Forbid();
            }
        }

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

    /// <summary>
    /// PUT /api/council-employee/appeals/{appealId}/assign
    /// Assigns an appeal to a specific council employee (Head only).
    /// </summary>
    [HttpPut("appeals/{appealId}/assign")]
    [ProducesResponseType(typeof(AppealResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignAppealToEmployee(Guid appealId, [FromBody] AssignAppealRequest request)
    {
        if (appealId == Guid.Empty || request == null || request.EmployeeId == Guid.Empty)
        {
            return BadRequest(new { success = false, message = "Invalid appeal ID or employee ID" });
        }

        var councilId = await GetCurrentCouncilIdAsync();
        if (!await IsCurrentUserCouncilHeadAsync())
        {
            return Forbid();
        }

        // Verify appeal belongs to this council
        var councilAppeals = await _appealService.GetAppealsByCouncilAsync(councilId);
        if (!councilAppeals.Any(a => a.AppealId == appealId))
        {
            return NotFound(new { success = false, message = "Appeal not found" });
        }

        // Verify employee belongs to this council
        var employeeCouncilId = await _employeeService.GetCouncilIdByEmployeeIdAsync(request.EmployeeId);
        if (employeeCouncilId == null || employeeCouncilId.Value != councilId)
        {
            return BadRequest(new { success = false, message = "Employee does not belong to this council" });
        }

        var updated = await _appealService.AssignAppealAsync(appealId, request.EmployeeId);
        if (updated == null)
        {
            return NotFound(new { success = false, message = "Appeal not found" });
        }

        return Ok(_mapper.Map<AppealResponse>(updated));
    }

    /// <summary>
    /// PUT /api/council-employee/appeals/{appealId}/unassign
    /// Unassigns an appeal.
    /// </summary>
    [HttpPut("appeals/{appealId}/unassign")]
    [ProducesResponseType(typeof(AppealResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassignAppeal(Guid appealId)
    {
        if (appealId == Guid.Empty)
        {
            return BadRequest(new { success = false, message = "Invalid appeal ID" });
        }

        await GetCurrentCouncilIdAsync();
        var appeal = await _appealService.UnassignAppealAsync(appealId);
        if (appeal == null)
        {
            return NotFound(new { success = false, message = "Appeal not found" });
        }

        return Ok(_mapper.Map<AppealResponse>(appeal));
    }

    #endregion

    #region Employee Management

    /// <summary>
    /// GET /api/council-employee/employees
    /// Gets all employees for the current council (Head only).
    /// </summary>
    [HttpGet("employees")]
    [ProducesResponseType(typeof(List<CouncilEmployeeListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCouncilEmployees()
    {
        if (!await IsCurrentUserCouncilHeadAsync())
        {
            return Forbid();
        }

        var councilId = await GetCurrentCouncilIdAsync();
        var employees = await _employeeService.GetCouncilEmployeesWithDetailsAsync(councilId);

        var response = employees.Select(e => new CouncilEmployeeListItemResponse
        {
            EmployeeId = e.EmployeeId,
            UserId = e.UserId,
            FullName = e.User?.FullName,
            Email = e.User?.Email,
            Position = e.Position
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// POST /api/council-employee/employees
    /// Registers a new council employee in the current employee's council.
    /// Only council heads can create new council employees.
    /// </summary>
    [HttpPost("employees")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterCouncilEmployee([FromBody] RegisterCouncilEmployeeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Validation failed",
                errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });
        }

        var councilId = await GetCurrentCouncilIdAsync();
        if (!await IsCurrentUserCouncilHeadAsync())
        {
            return Forbid();
        }

        try
        {
            var employeeId = await _employeeService.RegisterCouncilEmployeeAsync(
                councilId,
                request.FullName,
                request.Email,
                request.Password,
                request.Position);

            return StatusCode(StatusCodes.Status201Created, new
            {
                success = true,
                employeeId,
                councilId,
                message = "Council employee registered successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
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
        if (!await IsCurrentUserCouncilHeadAsync())
        {
            return Forbid();
        }

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

        var cleanOptions = request.Options?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (cleanOptions.Count < 2)
        {
            _logger.LogWarning("Poll must have at least 2 options");
            return BadRequest(new { success = false, message = "Poll must have at least 2 options" });
        }

        var councilId = await GetCurrentCouncilIdAsync();

        var createPollDto = new CreatePollDto
        {
            CouncilId = councilId,
            Title = request.Title,
            Description = request.Description,
            Options = cleanOptions
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

        if (!await IsCurrentUserCouncilHeadAsync())
        {
            return Forbid();
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

    /// <summary>
    /// GET /api/council-employee/statistics/overall
    /// Gets overall system appeal statistics.
    /// </summary>
    [HttpGet("statistics/overall")]
    [ProducesResponseType(typeof(AppealStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverallStatistics()
    {
        await GetCurrentCouncilIdAsync();
        var stats = await _appealService.GetAppealStatisticsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// GET /api/council-employee/statistics/council
    /// Gets statistics for current employee's council.
    /// </summary>
    [HttpGet("statistics/council")]
    [ProducesResponseType(typeof(AppealStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCouncilStatistics()
    {
        var councilId = await GetCurrentCouncilIdAsync();
        var stats = await _appealService.GetCouncilAppealStatisticsAsync(councilId);
        return Ok(stats);
    }

    /// <summary>
    /// GET /api/council-employee/statistics/employee
    /// Gets statistics for current employee.
    /// </summary>
    [HttpGet("statistics/employee")]
    [ProducesResponseType(typeof(AppealStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentEmployeeStatistics()
    {
        var currentUserId = GetCurrentUserId();
        var employee = await _employeeService.GetEmployeeByUserIdAsync(currentUserId);
        if (employee == null)
        {
            return Forbid();
        }

        var stats = await _appealService.GetEmployeeAppealStatisticsAsync(employee.EmployeeId);
        return Ok(stats);
    }

    /// <summary>
    /// GET /api/council-employee/statistics/status-distribution
    /// Gets overall status distribution.
    /// </summary>
    [HttpGet("statistics/status-distribution")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatusDistribution()
    {
        await GetCurrentCouncilIdAsync();
        var distribution = await _appealService.GetAppealStatusDistributionAsync();
        return Ok(distribution);
    }

    /// <summary>
    /// GET /api/council-employee/statistics/council/status-distribution
    /// Gets status distribution for current council.
    /// </summary>
    [HttpGet("statistics/council/status-distribution")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCouncilStatusDistribution()
    {
        var councilId = await GetCurrentCouncilIdAsync();
        var distribution = await _appealService.GetCouncilStatusDistributionAsync(councilId);
        return Ok(distribution);
    }

    /// <summary>
    /// GET /api/council-employee/statistics/average-resolution-time
    /// Gets average resolution time in days.
    /// </summary>
    [HttpGet("statistics/average-resolution-time")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAverageResolutionTime()
    {
        await GetCurrentCouncilIdAsync();
        var avgDays = await _appealService.GetAverageResolutionTimeInDaysAsync();
        return Ok(new { averageResolutionDays = avgDays });
    }

    /// <summary>
    /// GET /api/council-employee/statistics/trends
    /// Gets appeal trends for selected period.
    /// </summary>
    [HttpGet("statistics/trends")]
    [ProducesResponseType(typeof(AppealTrendDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTrends([FromQuery] int daysBack = 30)
    {
        if (daysBack <= 0)
        {
            return BadRequest(new { success = false, message = "Days back must be greater than 0" });
        }

        await GetCurrentCouncilIdAsync();
        var trends = await _appealService.GetAppealTrendAsync(daysBack);
        return Ok(trends);
    }

    /// <summary>
    /// GET /api/council-employee/appeals/{appealId}/summary
    /// Gets summary for a single appeal.
    /// </summary>
    [HttpGet("appeals/{appealId}/summary")]
    [ProducesResponseType(typeof(AppealSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppealSummary(Guid appealId)
    {
        if (appealId == Guid.Empty)
        {
            return BadRequest(new { success = false, message = "Invalid appeal ID" });
        }

        await GetCurrentCouncilIdAsync();
        var appeal = await _appealService.GetAppealByIdAsync(appealId);
        if (appeal == null)
        {
            return NotFound(new { success = false, message = "Appeal not found" });
        }

        return Ok(_mapper.Map<AppealSummaryResponse>(appeal));
    }

    /// <summary>
    /// GET /api/council-employee/appeals/summary/city/{city}
    /// Gets appeal summaries by city.
    /// </summary>
    [HttpGet("appeals/summary/city/{city}")]
    [ProducesResponseType(typeof(IEnumerable<AppealSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppealSummariesByCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest(new { success = false, message = "City is required" });
        }

        await GetCurrentCouncilIdAsync();
        var appeals = await _appealService.GetAppealsByCityAsync(city);
        return Ok(_mapper.Map<IEnumerable<AppealSummaryResponse>>(appeals));
    }

    /// <summary>
    /// GET /api/council-employee/appeals/summary/oblast/{oblast}
    /// Gets appeal summaries by oblast.
    /// </summary>
    [HttpGet("appeals/summary/oblast/{oblast}")]
    [ProducesResponseType(typeof(IEnumerable<AppealSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppealSummariesByOblast(string oblast)
    {
        if (string.IsNullOrWhiteSpace(oblast))
        {
            return BadRequest(new { success = false, message = "Oblast is required" });
        }

        await GetCurrentCouncilIdAsync();
        var appeals = await _appealService.GetAppealsByOblastAsync(oblast);
        return Ok(_mapper.Map<IEnumerable<AppealSummaryResponse>>(appeals));
    }

    /// <summary>
    /// GET /api/council-employee/appeals/summary/district/{district}
    /// Gets appeal summaries by district.
    /// </summary>
    [HttpGet("appeals/summary/district/{district}")]
    [ProducesResponseType(typeof(IEnumerable<AppealSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppealSummariesByDistrict(string district)
    {
        if (string.IsNullOrWhiteSpace(district))
        {
            return BadRequest(new { success = false, message = "District is required" });
        }

        await GetCurrentCouncilIdAsync();
        var appeals = await _appealService.GetAppealsByDistrictAsync(district);
        return Ok(_mapper.Map<IEnumerable<AppealSummaryResponse>>(appeals));
    }

    #endregion
}

