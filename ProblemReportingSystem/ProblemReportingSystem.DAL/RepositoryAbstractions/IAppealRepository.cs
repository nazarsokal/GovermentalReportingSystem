using ProblemReportingSystem.DAL.Entities;

namespace ProblemReportingSystem.DAL.RepositoryAbstractions;

public interface IAppealRepository : IProblemReportingSystemRepository<Appeal>
{
    Task<IEnumerable<Appeal>> GetAppealsByCouncilAsync(Guid councilId);

    Task<IEnumerable<Appeal>> GetAppealsByEmployeeAsync(Guid employeeId);

    Task<IEnumerable<Appeal>> GetAppealsByStatusAsync(Guid councilId, string status);
    Task<IEnumerable<Appeal>> GetAppealsByDistrictAsync(string district);

    Task<IEnumerable<Appeal>> GetAppealsByCityAsync(string city);

    Task<IEnumerable<Appeal>> GetAppealsByOblastAsync(string oblast);

    Task<Appeal?> GetAppealWithDetailsAsync(Guid appealId);

    Task<IEnumerable<Appeal>> GetAllAppealsWithDetailsAsync();

    Task<IEnumerable<Appeal>> GetUserAppealsWithDetailsAsync(Guid userId);

    Task<IEnumerable<Appeal>> GetUnassignedAppealsWithDetailsAsync();
}
