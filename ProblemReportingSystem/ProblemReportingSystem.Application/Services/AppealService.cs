using AutoMapper;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.RepositoryAbstractions;
using Microsoft.Extensions.Logging;

namespace ProblemReportingSystem.Application.Services;

public class AppealService : IAppealService
{
    private readonly IAppealRepository _appealRepository;
    private readonly IMapper _mapper;
    private readonly IProblemService _problemService;
    private readonly ILogger<AppealService> _logger;

    public AppealService(IAppealRepository appealRepository, IMapper mapper, IProblemService problemService, ILogger<AppealService> logger)
    {
        _appealRepository = appealRepository ?? throw new ArgumentNullException(nameof(appealRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _problemService = problemService ?? throw new ArgumentNullException(nameof(problemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Create
    public async Task<Guid> CreateAppealAsync(AppealDto createAppealDto)
    {
        if (createAppealDto == null)
            throw new ArgumentNullException(nameof(createAppealDto));

        var appeal = _mapper.Map<Appeal>(createAppealDto);
        var problem = await _problemService.CreateProblem(createAppealDto.ProblemDto);
        appeal.Problem = problem;
        
        await _appealRepository.CreateAsync(appeal);
        _logger.LogInformation($"Appeal created with ID: {appeal.AppealId} for user: {appeal.UserId}");
        return appeal.AppealId;
    }

    // Read - Single
    public async Task<AppealDto?> GetAppealByIdAsync(Guid appealId)
    {
        if (appealId == Guid.Empty)
            throw new ArgumentException("Appeal ID cannot be empty", nameof(appealId));

        var appeal = await _appealRepository.GetAppealWithDetailsAsync(appealId);
        if (appeal == null)
        {
            _logger.LogWarning($"Appeal with ID {appealId} not found");
            return null;
        }

        return _mapper.Map<AppealDto>(appeal);
    }

    // Read - All
    public async Task<IEnumerable<AppealDto>> GetAllAppealsAsync()
    {
        _logger.LogInformation("Retrieving all appeals");
        var appeals = await _appealRepository.GetAllAppealsWithDetailsAsync();
        return _mapper.Map<IEnumerable<AppealDto>>(appeals);
    }

    // Read - By Council
    public async Task<IEnumerable<AppealDto>> GetAppealsByCouncilAsync(Guid councilId)
    {
        if (councilId == Guid.Empty)
            throw new ArgumentException("Council ID cannot be empty", nameof(councilId));

        _logger.LogInformation($"Retrieving appeals for council: {councilId}");
        var appeals = await _appealRepository.GetAppealsByCouncilAsync(councilId);
        return _mapper.Map<IEnumerable<AppealDto>>(appeals);
    }

    // Read - By Employee
    public async Task<IEnumerable<AppealDto>> GetCouncilAppealsByEmployeeAsync(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID cannot be empty", nameof(employeeId));

        _logger.LogInformation($"Retrieving appeals assigned to employee: {employeeId}");
        var appeals = await _appealRepository.GetAppealsByEmployeeAsync(employeeId);
        return _mapper.Map<IEnumerable<AppealDto>>(appeals);
    }

    // Read - By Status and Council
    public async Task<IEnumerable<AppealDto>> GetAppealsByStatusAsync(Guid councilId, string status)
    {
        if (councilId == Guid.Empty)
            throw new ArgumentException("Council ID cannot be empty", nameof(councilId));
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be null or empty", nameof(status));

        _logger.LogInformation($"Retrieving appeals for council {councilId} with status: {status}");
        var appeals = await _appealRepository.GetAppealsByStatusAsync(councilId, status);
        return _mapper.Map<IEnumerable<AppealDto>>(appeals);
    }

    // Read - By User
    public async Task<IEnumerable<AppealDto>> GetUserAppealsAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        _logger.LogInformation($"Retrieving appeals for user: {userId}");
        var appeals = await _appealRepository.GetUserAppealsWithDetailsAsync(userId);
        return _mapper.Map<IEnumerable<AppealDto>>(appeals);
    }

    // Read - Unassigned Appeals
    public async Task<IEnumerable<AppealDto>> GetUnassignedAppealsAsync()
    {
        _logger.LogInformation("Retrieving unassigned appeals");
        var appeals = await _appealRepository.GetUnassignedAppealsWithDetailsAsync();
        return _mapper.Map<IEnumerable<AppealDto>>(appeals);
    }

    // Update - Status
    public async Task<AppealDto?> UpdateAppealStatusAsync(Guid appealId, string status)
    {
        if (appealId == Guid.Empty)
            throw new ArgumentException("Appeal ID cannot be empty", nameof(appealId));
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be null or empty", nameof(status));

        var appeal = await _appealRepository.GetByIdAsync(appealId);
        if (appeal == null)
        {
            _logger.LogWarning($"Appeal with ID {appealId} not found");
            return null;
        }

        appeal.Problem.Status = status;
        appeal.UpdatedAt = DateTime.UtcNow;

        await _appealRepository.UpdateAsync(appeal);
        _logger.LogInformation($"Appeal {appealId} status updated to: {status}");

        return _mapper.Map<AppealDto>(appeal);
    }

    // Update - Assign Employee
    public async Task<AppealDto?> AssignAppealAsync(Guid appealId, Guid employeeId)
    {
        if (appealId == Guid.Empty)
            throw new ArgumentException("Appeal ID cannot be empty", nameof(appealId));
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID cannot be empty", nameof(employeeId));

        var appeal = await _appealRepository.GetByIdAsync(appealId);
        if (appeal == null)
        {
            _logger.LogWarning($"Appeal with ID {appealId} not found");
            return null;
        }

        appeal.AssignedEmployeeId = employeeId;
        appeal.UpdatedAt = DateTime.UtcNow;

        await _appealRepository.UpdateAsync(appeal);
        _logger.LogInformation($"Appeal {appealId} assigned to employee {employeeId}");

        return _mapper.Map<AppealDto>(appeal);
    }

    // Update - Unassign Employee
    public async Task<AppealDto?> UnassignAppealAsync(Guid appealId)
    {
        if (appealId == Guid.Empty)
            throw new ArgumentException("Appeal ID cannot be empty", nameof(appealId));

        var appeal = await _appealRepository.GetByIdAsync(appealId);
        if (appeal == null)
        {
            _logger.LogWarning($"Appeal with ID {appealId} not found");
            return null;
        }

        appeal.AssignedEmployeeId = null;
        appeal.UpdatedAt = DateTime.UtcNow;

        await _appealRepository.UpdateAsync(appeal);
        _logger.LogInformation($"Appeal {appealId} has been unassigned");

        return _mapper.Map<AppealDto>(appeal);
    }

    // Delete
    public async Task<bool> DeleteAppealAsync(Guid appealId)
    {
        if (appealId == Guid.Empty)
            throw new ArgumentException("Appeal ID cannot be empty", nameof(appealId));

        var appeal = await _appealRepository.GetByIdAsync(appealId);
        if (appeal == null)
        {
            _logger.LogWarning($"Appeal with ID {appealId} not found for deletion");
            return false;
        }

        await _appealRepository.DeleteAsync(appeal);
        _logger.LogInformation($"Appeal {appealId} has been deleted");
        return true;
    }

    // Statistics - Overall
    public async Task<AppealStatisticsDto> GetAppealStatisticsAsync()
    {
        _logger.LogInformation("Calculating overall appeal statistics");
        
        var appealsList = _appealRepository.GetAll().ToList();

        var stats = new AppealStatisticsDto
        {
            TotalAppeals = appealsList.Count,
            ResolvedAppeals = appealsList.Count(a => a.Problem.Status == "Resolved"),
            PendingAppeals = appealsList.Count(a => a.Problem.Status == "Pending"),
            AssignedAppeals = appealsList.Count(a => a.AssignedEmployeeId.HasValue),
            UnassignedAppeals = appealsList.Count(a => !a.AssignedEmployeeId.HasValue),
            ResolutionRate = appealsList.Count > 0 
                ? (double)appealsList.Count(a => a.Problem.Status == "Resolved") / appealsList.Count * 100 
                : 0,
            LastUpdated = DateTime.UtcNow
        };

        // Calculate average resolution time
        var resolvedAppeals = appealsList.Where(a => a.Problem.Status == "Resolved").ToList();
        if (resolvedAppeals.Any())
        {
            var avgDays = resolvedAppeals
                .Where(a => a.CreatedAt.HasValue && a.UpdatedAt.HasValue)
                .Average(a => (double)(a.UpdatedAt!.Value - a.CreatedAt!.Value).TotalDays);
            stats.AverageResolutionDays = (int)avgDays;
        }

        return stats;
    }

    // Statistics - By Council
    public async Task<AppealStatisticsDto> GetCouncilAppealStatisticsAsync(Guid councilId)
    {
        if (councilId == Guid.Empty)
            throw new ArgumentException("Council ID cannot be empty", nameof(councilId));

        _logger.LogInformation($"Calculating appeal statistics for council: {councilId}");
        
        var appeals = await _appealRepository.GetAppealsByCouncilAsync(councilId);
        var appealsList = appeals.ToList();

        var stats = new AppealStatisticsDto
        {
            TotalAppeals = appealsList.Count,
            ResolvedAppeals = appealsList.Count(a => a.Problem.Status == "Resolved"),
            PendingAppeals = appealsList.Count(a => a.Problem.Status == "Pending"),
            AssignedAppeals = appealsList.Count(a => a.AssignedEmployeeId.HasValue),
            UnassignedAppeals = appealsList.Count(a => !a.AssignedEmployeeId.HasValue),
            ResolutionRate = appealsList.Count > 0 
                ? (double)appealsList.Count(a => a.Problem.Status == "Resolved") / appealsList.Count * 100 
                : 0,
            LastUpdated = DateTime.UtcNow
        };

        var resolvedAppeals = appealsList.Where(a => a.Problem.Status == "Resolved").ToList();
        if (resolvedAppeals.Any())
        {
            var avgDays = resolvedAppeals
                .Where(a => a.CreatedAt.HasValue && a.UpdatedAt.HasValue)
                .Average(a => (double)(a.UpdatedAt!.Value - a.CreatedAt!.Value).TotalDays);
            stats.AverageResolutionDays = (int)avgDays;
        }

        return stats;
    }

    // Statistics - By Employee
    public async Task<AppealStatisticsDto> GetEmployeeAppealStatisticsAsync(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID cannot be empty", nameof(employeeId));

        _logger.LogInformation($"Calculating appeal statistics for employee: {employeeId}");
        
        var appeals = await _appealRepository.GetAppealsByEmployeeAsync(employeeId);
        var appealsList = appeals.ToList();

        var stats = new AppealStatisticsDto
        {
            TotalAppeals = appealsList.Count,
            ResolvedAppeals = appealsList.Count(a => a.Problem.Status == "Resolved"),
            PendingAppeals = appealsList.Count(a => a.Problem.Status == "Pending"),
            AssignedAppeals = appealsList.Count,
            UnassignedAppeals = 0,
            ResolutionRate = appealsList.Count > 0 
                ? (double)appealsList.Count(a => a.Problem.Status == "Resolved") / appealsList.Count * 100 
                : 0,
            LastUpdated = DateTime.UtcNow
        };

        var resolvedAppeals = appealsList.Where(a => a.Problem.Status == "Resolved").ToList();
        if (resolvedAppeals.Any())
        {
            var avgDays = resolvedAppeals
                .Where(a => a.CreatedAt.HasValue && a.UpdatedAt.HasValue)
                .Average(a => (double)(a.UpdatedAt!.Value - a.CreatedAt!.Value).TotalDays);
            stats.AverageResolutionDays = (int)avgDays;
        }

        return stats;
    }

    // Statistics - Status Distribution
    public async Task<Dictionary<string, int>> GetAppealStatusDistributionAsync()
    {
        _logger.LogInformation("Calculating appeal status distribution");
        
        var appeals = _appealRepository.GetAll();
        return appeals
            .GroupBy(a => a.Problem.Status ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Statistics - Council Status Distribution
    public async Task<Dictionary<string, int>> GetCouncilStatusDistributionAsync(Guid councilId)
    {
        if (councilId == Guid.Empty)
            throw new ArgumentException("Council ID cannot be empty", nameof(councilId));

        _logger.LogInformation($"Calculating status distribution for council: {councilId}");
        
        var appeals = await _appealRepository.GetAppealsByCouncilAsync(councilId);
        return appeals
            .GroupBy(a => a.Problem.Status ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Statistics - Average Resolution Time
    public async Task<int> GetAverageResolutionTimeInDaysAsync()
    {
        _logger.LogInformation("Calculating average resolution time");
        
        var appeals = _appealRepository.GetAll();
        var resolvedAppeals = appeals
            .Where(a => a.Problem.Status == "Resolved" && a.CreatedAt.HasValue && a.UpdatedAt.HasValue)
            .ToList();

        if (!resolvedAppeals.Any())
            return 0;

        var avgDays = resolvedAppeals
            .Average(a => (a.UpdatedAt.Value - a.CreatedAt.Value).TotalDays);

        return (int)avgDays;
    }

    // Statistics - Trends
    public async Task<AppealTrendDto> GetAppealTrendAsync(int daysBack = 30)
    {
        if (daysBack <= 0)
            throw new ArgumentException("Days back must be greater than 0", nameof(daysBack));

        _logger.LogInformation($"Calculating appeal trends for the last {daysBack} days");
        
        var appeals = _appealRepository.GetAll();
        var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);
        
        var recentAppeals = appeals
            .Where(a => a.CreatedAt.HasValue && a.CreatedAt.Value >= cutoffDate)
            .ToList();

        return new AppealTrendDto
        {
            Date = DateTime.UtcNow,
            CreatedCount = recentAppeals.Count,
            ResolvedCount = recentAppeals.Count(a => a.Problem.Status == "Resolved"),
            PendingCount = recentAppeals.Count(a => a.Problem.Status == "Pending")
        };
    }
}
