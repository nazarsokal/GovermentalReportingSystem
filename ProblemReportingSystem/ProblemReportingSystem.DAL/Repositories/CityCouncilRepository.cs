using Microsoft.EntityFrameworkCore;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.DAL.Repositories;

public class CityCouncilRepository : ProblemReportingSystemRepository<CityCouncil>, ICityCouncilRepository
{
    private readonly ProblemReportingSystemDbContext _context;

    public CityCouncilRepository(ProblemReportingSystemDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<CityCouncil>> GetAllCouncilsAsync()
    {
        return await _context.CityCouncils
            .Include(c => c.Address)
            .Include(c => c.CouncilEmployees)
            .ToListAsync();
    }

    public async Task<CityCouncil?> GetCouncilByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.CityCouncils
            .Include(c => c.Address)
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<CityCouncil?> GetCouncilWithDetailsAsync(Guid councilId)
    {
        return await _context.CityCouncils
            .Include(c => c.Address)
            .Include(c => c.CouncilEmployees)
            .ThenInclude(ce => ce.User)
            .Include(c => c.Polls)
            .FirstOrDefaultAsync(c => c.CouncilId == councilId);
    }
}

