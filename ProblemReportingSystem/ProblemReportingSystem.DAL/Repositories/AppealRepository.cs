using Microsoft.EntityFrameworkCore;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.DAL.Repositories;

public class AppealRepository : ProblemReportingSystemRepository<Appeal>, IAppealRepository
{
    private readonly ProblemReportingSystemDbContext _context;

    public AppealRepository(ProblemReportingSystemDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Appeal>> GetAppealsByCouncilAsync(Guid councilId)
    {
        return await _context.Appeals
            .Where(a => a.AssignedEmployee != null && a.AssignedEmployee.CouncilId == councilId)
            .Include(a => a.Problem)
            .Include(a => a.AssignedEmployee)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appeal>> GetAppealsByEmployeeAsync(Guid employeeId)
    {
        return await _context.Appeals
            .Where(a => a.AssignedEmployeeId == employeeId)
            .Include(a => a.Problem)
            .ThenInclude(p => p.Address)
            .Include(a => a.Problem)
            .ThenInclude(p => p.Category)
            .Include(a => a.AssignedEmployee)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appeal>> GetAppealsByStatusAsync(Guid councilId, string status)
    {
        return await _context.Appeals
            .Where(a => a.AssignedEmployee != null && 
                        a.AssignedEmployee.CouncilId == councilId &&
                        a.Problem.Status == status)
            .Include(a => a.Problem)
            .Include(a => a.AssignedEmployee)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appeal>> GetAppealsByDistrictAsync(string district)
    {
        var appeals = await _context.Appeals
            .Include(a => a.Problem)
            .ThenInclude(p => p.Address)
            .Where(a => a.Problem.Address.District == district).ToListAsync();
        
        return appeals;
    }

    public async Task<IEnumerable<Appeal>> GetAppealsByCityAsync(string city)
    {
        var appeals = await _context.Appeals
            .Include(a => a.Problem)
            .ThenInclude(p => p.Address)
            .Where(a => a.Problem.Address.City == city).ToListAsync();
        
        return appeals;
    }

    public async Task<IEnumerable<Appeal>> GetAppealsByOblastAsync(string oblast)
    {
        var appeals = await _context.Appeals
            .Include(a => a.Problem)
            .ThenInclude(p => p.Address)
            .Where(a => a.Problem.Address.Oblast == oblast).ToListAsync();
        
        return appeals;
    }

    public async Task<Appeal?> GetAppealWithDetailsAsync(Guid appealId)
    {
        return await _context.Appeals
            .Include(a => a.Problem)
            .ThenInclude(p => p.Address)
            .Include(a => a.Problem)
            .ThenInclude(p => p.Category)
            .Include(a => a.Problem)
            .ThenInclude(p => p.ProblemPhotos)
            .Include(a => a.AssignedEmployee)
            .ThenInclude(ae => ae.User)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.AppealId == appealId);
    }

    public async Task<IEnumerable<Appeal>> GetAllAppealsWithDetailsAsync()
    {
        return await _context.Appeals
            .Include(a => a.Problem)
            .ThenInclude(p => p.Address)
            .Include(a => a.Problem)
            .ThenInclude(p => p.Category)
            .Include(a => a.Problem)
            .ThenInclude(p => p.ProblemPhotos)
            .Include(a => a.AssignedEmployee)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appeal>> GetUserAppealsWithDetailsAsync(Guid userId)
    {
        return await _context.Appeals
            .Where(a => a.UserId == userId)
            .Include(a => a.Problem)
            .ThenInclude(p => p.Address)
            .Include(a => a.Problem)
            .ThenInclude(p => p.Category)
            .Include(a => a.Problem)
            .ThenInclude(p => p.ProblemPhotos)
            .Include(a => a.AssignedEmployee)
            .Include(a => a.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appeal>> GetUnassignedAppealsWithDetailsAsync()
    {
        return await _context.Appeals
            .Where(a => a.AssignedEmployeeId == null)
            .Include(a => a.Problem)
            .ThenInclude(p => p.Address)
            .Include(a => a.Problem)
            .ThenInclude(p => p.Category)
            .Include(a => a.Problem)
            .ThenInclude(p => p.ProblemPhotos)
            .Include(a => a.AssignedEmployee)
            .Include(a => a.User)
            .ToListAsync();
    }
}
