using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.DAL.Repositories;

/// <summary>
/// Repository implementation for ProblemCategory entity operations
/// </summary>
public class ProblemCategoryRepository : ProblemReportingSystemRepository<ProblemCategory>, IProblemCategoryRepository
{
    public ProblemCategoryRepository(ProblemReportingSystemDbContext context) : base(context)
    {
    }
}

