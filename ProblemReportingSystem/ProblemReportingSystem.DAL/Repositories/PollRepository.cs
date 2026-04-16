using Microsoft.EntityFrameworkCore;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.DAL.Repositories;

public class PollRepository : ProblemReportingSystemRepository<Poll>, IPollRepository
{
    private readonly ProblemReportingSystemDbContext _context;

    public PollRepository(ProblemReportingSystemDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Poll>> GetCouncilPollsAsync(Guid councilId)
    {
        return await _context.Polls
            .Where(p => p.CouncilId == councilId)
            .Include(p => p.PollOptions)
            .ThenInclude(po => po.PollVotes)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    public async Task<Poll?> GetPollWithDetailsAsync(Guid pollId)
    {
        return await _context.Polls
            .Include(p => p.Council)
            .Include(p => p.PollOptions)
            .FirstOrDefaultAsync(p => p.PollId == pollId);
    }

    public async Task<Poll?> GetPollWithVotesAsync(Guid pollId)
    {
        return await _context.Polls
            .Include(p => p.PollOptions)
            .ThenInclude(po => po.PollVotes)
            .FirstOrDefaultAsync(p => p.PollId == pollId);
    }
}

