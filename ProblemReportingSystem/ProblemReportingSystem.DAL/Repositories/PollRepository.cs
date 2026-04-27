using Microsoft.EntityFrameworkCore;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.RepositoryAbstractions;
using System.Linq;

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
            .ThenInclude(c => c.Address)
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

    public async Task<IEnumerable<Poll>> GetActivePollsByDistrictAsync(string district)
    {
        return await _context.Polls
            .Include(p => p.Council)
            .ThenInclude(c => c.Address)
            .Include(p => p.PollOptions)
            .ThenInclude(po => po.PollVotes)
            .Where(p => p.IsActive == true
                        && p.Council.Address != null
                        && p.Council.Address.District != null)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    public async Task<bool> UserHasVotedInPollAsync(Guid pollId, Guid userId)
    {
        return await _context.PollVotes
            .AnyAsync(v => v.UserId == userId && v.Option.PollId == pollId);
    }

    public async Task<bool> OptionBelongsToPollAsync(Guid pollId, Guid optionId)
    {
        return await _context.PollOptions
            .AnyAsync(o => o.OptionId == optionId && o.PollId == pollId);
    }

    public async Task CreateVoteAsync(PollVote vote)
    {
        await _context.PollVotes.AddAsync(vote);
        await _context.SaveChangesAsync();
    }
}

