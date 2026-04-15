using ProblemReportingSystem.Application.DTOs;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

public interface IProblemService
{
    public Task<Guid> CreateProblem(CreateProblemDto createProblemDto);
    
    public Task<Guid> UpdateProblem(UpdateProblemDto updateProblemDto);
}