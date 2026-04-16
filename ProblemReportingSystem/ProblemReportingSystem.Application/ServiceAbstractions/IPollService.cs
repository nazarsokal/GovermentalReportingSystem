using ProblemReportingSystem.Application.DTOs;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

public interface IPollService
{
    Task<Guid> CreatePollAsync(CreatePollDto createPollDto);

    Task<IEnumerable<PollDetailsDto>> GetCouncilPollsAsync(Guid councilId);

    Task<PollDetailsDto?> GetPollByIdAsync(Guid pollId);

    Task<bool> ClosePollAsync(Guid pollId);

    Task<PollDetailsDto?> GetPollWithResultsAsync(Guid pollId);
}

