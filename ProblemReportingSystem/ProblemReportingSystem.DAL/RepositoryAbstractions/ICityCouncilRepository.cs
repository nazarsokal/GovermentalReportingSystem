using ProblemReportingSystem.DAL.Entities;

namespace ProblemReportingSystem.DAL.RepositoryAbstractions;

public interface ICityCouncilRepository : IProblemReportingSystemRepository<CityCouncil>
{
    Task<IEnumerable<CityCouncil>> GetAllCouncilsAsync();

    Task<CityCouncil?> GetCouncilByNameAsync(string name);

    Task<CityCouncil?> GetCouncilWithDetailsAsync(Guid councilId);
}

