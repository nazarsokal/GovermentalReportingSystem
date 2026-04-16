using ProblemReportingSystem.DAL.Entities;

namespace ProblemReportingSystem.DAL.RepositoryAbstractions;

public interface ICouncilEmployeeRepository : IProblemReportingSystemRepository<CouncilEmployee>
{
    Task<CouncilEmployee?> GetEmployeeByUserIdAsync(Guid userId);

    Task<IEnumerable<CouncilEmployee>> GetCouncilEmployeesAsync(Guid councilId);

    Task<CouncilEmployee?> GetEmployeeWithDetailsAsync(Guid employeeId);
}

