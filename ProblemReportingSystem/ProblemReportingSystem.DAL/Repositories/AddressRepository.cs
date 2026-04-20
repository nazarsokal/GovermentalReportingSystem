using Microsoft.EntityFrameworkCore;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.DAL.Repositories;

public class AddressRepository : ProblemReportingSystemRepository<Address>, IAddressRepository
{
    private readonly ProblemReportingSystemDbContext _context;

    public AddressRepository(ProblemReportingSystemDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Get all unique oblasts from addresses
    /// </summary>
    public async Task<List<string>> GetAllOblastsAsync()
    {
        return await _context.Addresses
            .Where(a => a.Oblast != null && a.Oblast != string.Empty)
            .Select(a => a.Oblast!)
            .Distinct()
            .OrderBy(o => o)
            .ToListAsync();
    }

    /// <summary>
    /// Get all unique districts from addresses
    /// </summary>
    public async Task<List<string>> GetAllDistrictsAsync()
    {
        return await _context.Addresses
            .Where(a => a.District != null && a.District != string.Empty)
            .Select(a => a.District!)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }

    /// <summary>
    /// Get all unique districts for a specific oblast
    /// </summary>
    public async Task<List<string>> GetDistrictsByOblastAsync(string oblast)
    {
        return await _context.Addresses
            .Where(a => a.Oblast == oblast && a.District != null && a.District != string.Empty)
            .Select(a => a.District!)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }
}

