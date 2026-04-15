using ProblemReportingSystem.DAL.Entities;

namespace ProblemReportingSystem.DAL.RepositoryAbstractions;

public interface IProblemRepository : IProblemReportingSystemRepository<Problem>
{
    Task<Problem?> GetProblemWithDetailsAsync(Guid problemId);
    
    Task<IEnumerable<Problem>> GetUserProblemsAsync(Guid userId);
    
    Task<IEnumerable<Problem>> GetProblemsByStatusAsync(string status);
    
    Task<IEnumerable<Problem>> GetProblemsByCategoryAsync(Guid categoryId);
    
    Task<IEnumerable<Problem>> GetProblemsInBoundsAsync(decimal minLat, decimal maxLat, decimal minLng, decimal maxLng);
    
    Task<IEnumerable<Problem>> GetAssignedProblemsAsync(Guid employeeId);
}