using ProblemReportingSystem.DAL.Entities;

namespace ProblemReportingSystem.DAL.RepositoryAbstractions;

public interface IPollRepository : IProblemReportingSystemRepository<Poll>
{
    Task<IEnumerable<Poll>> GetCouncilPollsAsync(Guid councilId);

    Task<Poll?> GetPollWithDetailsAsync(Guid pollId);

    Task<Poll?> GetPollWithVotesAsync(Guid pollId);

    Task<IEnumerable<Poll>> GetActivePollsByDistrictAsync(string district);

    Task<bool> UserHasVotedInPollAsync(Guid pollId, Guid userId);

    Task<bool> OptionBelongsToPollAsync(Guid pollId, Guid optionId);

    Task CreateVoteAsync(PollVote vote);
}

