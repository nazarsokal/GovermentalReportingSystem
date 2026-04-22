using ProblemReportingSystem.Application.DTOs;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

public interface IAppealService
{
    // Create
    Task<Guid> CreateAppealAsync(AppealDto createAppealDto);

    // Read
    Task<AppealDto?> GetAppealByIdAsync(Guid appealId);
    Task<IEnumerable<AppealDto>> GetAllAppealsAsync();
    Task<IEnumerable<AppealDto>> GetAppealsByDistrictAsync(string distinct);
    Task<IEnumerable<AppealDto>> GetCouncilAppealsByEmployeeAsync(Guid employeeId);
    Task<IEnumerable<AppealDto>> GetAppealsByCouncilAsync(Guid councilId);
    Task<IEnumerable<AppealDto>> GetAppealsByStatusAsync(Guid councilId, string status);
    Task<IEnumerable<AppealDto>> GetUserAppealsAsync(Guid userId);
    Task<IEnumerable<AppealDto>> GetUnassignedAppealsAsync();

    // Update
    Task<AppealDto?> UpdateAppealStatusAsync(Guid appealId, string status);
    Task<AppealDto?> AssignAppealAsync(Guid appealId, Guid employeeId);
    Task<AppealDto?> UnassignAppealAsync(Guid appealId);

    // Delete
    Task<bool> DeleteAppealAsync(Guid appealId);

    // Statistics
    Task<AppealStatisticsDto> GetAppealStatisticsAsync();
    Task<AppealStatisticsDto> GetCouncilAppealStatisticsAsync(Guid councilId);
    Task<AppealStatisticsDto> GetEmployeeAppealStatisticsAsync(Guid employeeId);
    Task<Dictionary<string, int>> GetAppealStatusDistributionAsync();
    Task<Dictionary<string, int>> GetCouncilStatusDistributionAsync(Guid councilId);
    Task<int> GetAverageResolutionTimeInDaysAsync();
    Task<AppealTrendDto> GetAppealTrendAsync(int daysBack = 30);
}
