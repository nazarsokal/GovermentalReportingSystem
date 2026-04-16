using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.DAL.Entities;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

public interface IProblemService
{
    public Task<Problem> CreateProblem(CreateProblemDto createProblemDto);
    
    public Task<Guid> UpdateProblem(UpdateProblemDto updateProblemDto);
    
    public Task<IEnumerable<ProblemDto>> GetAllProblems();
    
    public Task<ProblemDto> GetProblemByIdAsync(Guid problemId);
    
    public Task<IEnumerable<ProblemDto>> GetUserProblems(Guid userId);
    
    public Task<IEnumerable<ProblemDto>> GetProblemsByStatus(string status);
    
    public Task<IEnumerable<ProblemDto>> GetProblemsByCategory(Guid categoryId);
    
    public Task<IEnumerable<ProblemDto>> GetProblemsInBounds(decimal minLat, decimal maxLat, decimal minLng, decimal maxLng);
    
    public Task<IEnumerable<ProblemDto>> GetAssignedProblems(Guid employeeId);

}