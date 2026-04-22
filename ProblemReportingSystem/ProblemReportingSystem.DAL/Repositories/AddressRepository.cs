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

    /// <summary>
    /// Get all unique districts with their coordinates
    /// Prioritizes coordinates from addresses where city councils are located
    /// </summary>
    public async Task<List<(string District, decimal Latitude, decimal Longitude)>> GetDistrictsWithCoordinatesAsync()
    {
        // Get all districts with city councils first
        var councilAddresses = await _context.CityCouncils
            .Where(cc => cc.Address != null && cc.Address.District != null && cc.Address.District != string.Empty)
            .Select(cc => new { cc.Address!.District, cc.Address!.Latitude, cc.Address!.Longitude })
            .Distinct()
            .ToListAsync();

        // Get all other addresses with districts
        var otherAddresses = await _context.Addresses
            .Where(a => a.District != null && a.District != string.Empty)
            .Select(a => new { a.District, a.Latitude, a.Longitude })
            .Distinct()
            .ToListAsync();

        // Combine and prioritize council addresses
        var result = new Dictionary<string, (decimal, decimal)>();

        // First add council addresses (these will have priority)
        foreach (var addr in councilAddresses)
        {
            if (!result.ContainsKey(addr.District!))
            {
                result[addr.District!] = (addr.Latitude, addr.Longitude);
            }
        }

        // Then add other addresses for districts not yet in the list
        foreach (var addr in otherAddresses)
        {
            if (!result.ContainsKey(addr.District!))
            {
                result[addr.District!] = (addr.Latitude, addr.Longitude);
            }
        }

        // Convert to sorted list
        return result
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => (kvp.Key, kvp.Value.Item1, kvp.Value.Item2))
            .ToList();
    }
}
