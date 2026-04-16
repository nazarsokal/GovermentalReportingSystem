using ProblemReportingSystem.Application.DTOs;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

public interface IStatisticsService
{
    Task<StatisticsDto> GetCouncilKPIAsync(Guid councilId);

    Task<EfficiencyReportDto?> GetCouncilEfficiencyAsync(Guid councilId);

    Task<IEnumerable<EfficiencyReportDto>> GetAllCouncilsEfficiencyAsync();
}

