using ProblemReportingSystem.DAL.Entities;

namespace ProblemReportingSystem.DAL.RepositoryAbstractions;

public interface IPollRepository : IProblemReportingSystemRepository<Poll>
{
    Task<IEnumerable<Poll>> GetCouncilPollsAsync(Guid councilId);

    Task<Poll?> GetPollWithDetailsAsync(Guid pollId);

    Task<Poll?> GetPollWithVotesAsync(Guid pollId);
}

