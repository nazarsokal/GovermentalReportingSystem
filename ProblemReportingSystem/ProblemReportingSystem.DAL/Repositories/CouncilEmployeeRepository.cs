using Microsoft.EntityFrameworkCore;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.DAL.Repositories;

public class CouncilEmployeeRepository : ProblemReportingSystemRepository<CouncilEmployee>, ICouncilEmployeeRepository
{
    private readonly ProblemReportingSystemDbContext _context;

    public CouncilEmployeeRepository(ProblemReportingSystemDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<CouncilEmployee?> GetEmployeeByUserIdAsync(Guid userId)
    {
        return await _context.CouncilEmployees
            .Include(ce => ce.User)
            .Include(ce => ce.Council)
            .FirstOrDefaultAsync(ce => ce.User.UserId == userId);
    }

    public async Task<IEnumerable<CouncilEmployee>> GetCouncilEmployeesAsync(Guid councilId)
    {
        return await _context.CouncilEmployees
            .Where(ce => ce.CouncilId == councilId)
            .Include(ce => ce.User)
            .Include(ce => ce.Council)
            .ToListAsync();
    }

    public async Task<CouncilEmployee?> GetEmployeeWithDetailsAsync(Guid employeeId)
    {
        return await _context.CouncilEmployees
            .Include(ce => ce.User)
            .Include(ce => ce.Council)
            .Include(ce => ce.Appeals)
            .FirstOrDefaultAsync(ce => ce.EmployeeId == employeeId);
    }
}
