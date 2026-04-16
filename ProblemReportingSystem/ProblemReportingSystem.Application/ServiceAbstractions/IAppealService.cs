using ProblemReportingSystem.Application.DTOs;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

public interface IAppealService
{
    Task<IEnumerable<AppealDto>> GetCouncilAppealsByEmployeeAsync(Guid employeeId);

    Task<IEnumerable<AppealDto>> GetAppealsByCouncilAsync(Guid councilId);

    Task<IEnumerable<AppealDto>> GetAppealsByStatusAsync(Guid councilId, string status);

    Task<AppealDto?> UpdateAppealStatusAsync(Guid appealId, string status);

    Task<AppealDto?> AssignAppealAsync(Guid appealId, Guid employeeId);
}

