using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.DAL.Repositories;

public class ProblemRepository : ProblemReportingSystemRepository<Problem>, IProblemRepository
{
    private readonly ProblemReportingSystemDbContext _context;

    public ProblemRepository(ProblemReportingSystemDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Task<Problem?> GetProblemWithDetailsAsync(Guid problemId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Problem>> GetUserProblemsAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Problem>> GetProblemsByStatusAsync(string status)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Problem>> GetProblemsByCategoryAsync(Guid categoryId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Problem>> GetProblemsInBoundsAsync(decimal minLat, decimal maxLat, decimal minLng, decimal maxLng)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Problem>> GetAssignedProblemsAsync(Guid employeeId)
    {
        throw new NotImplementedException();
    }
}