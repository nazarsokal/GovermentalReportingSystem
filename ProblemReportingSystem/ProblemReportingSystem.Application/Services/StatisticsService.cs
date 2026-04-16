using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Infrastructure;

namespace ProblemReportingSystem.Application.Services;

public class StatisticsService : IStatisticsService
{
    private readonly ProblemReportingSystemDbContext _context;
    private readonly IMapper _mapper;

    public StatisticsService(ProblemReportingSystemDbContext context, IMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<StatisticsDto> GetCouncilKPIAsync(Guid councilId)
    {
        if (councilId == Guid.Empty)
            throw new ArgumentException("Council ID cannot be empty", nameof(councilId));

        var appeals = await _context.Appeals
            .Where(a => a.AssignedEmployee != null && a.AssignedEmployee.CouncilId == councilId)
            .Include(a => a.Problem)
            .ToListAsync();

        var totalAppeals = appeals.Count;
        var pendingAppeals = appeals.Count(a => a.Problem.Status == "Pending");
        var resolvedAppeals = appeals.Count(a => a.Problem.Status == "Resolved");

        var resolvedAppealsList = appeals
            .Where(a => a.Problem.Status == "Resolved")
            .ToList();

        decimal averageResolutionDays = 0;
        if (resolvedAppealsList.Any())
        {
            var totalDays = resolvedAppealsList.Sum(a =>
                (a.Problem.CreatedAt.HasValue && a.UpdatedAt.HasValue)
                    ? (a.UpdatedAt.Value - a.Problem.CreatedAt.Value).TotalDays
                    : 0);

            averageResolutionDays = (decimal)(totalDays / resolvedAppealsList.Count);
        }

        return new StatisticsDto
        {
            TotalAppeals = totalAppeals,
            PendingAppeals = pendingAppeals,
            ResolvedAppeals = resolvedAppeals,
            AverageResolutionDays = averageResolutionDays
        };
    }

    public async Task<EfficiencyReportDto?> GetCouncilEfficiencyAsync(Guid councilId)
    {
        if (councilId == Guid.Empty)
            throw new ArgumentException("Council ID cannot be empty", nameof(councilId));

        var council = await _context.CityCouncils
            .FirstOrDefaultAsync(c => c.CouncilId == councilId);

        if (council == null)
            return null;

        var efficiency = await _context.VCouncilEfficiencyReports
            .FirstOrDefaultAsync(r => r.CouncilId == councilId);

        if (efficiency == null)
        {
            return new EfficiencyReportDto
            {
                CouncilId = councilId,
                CouncilName = council.Name,
                TotalAppeals = 0,
                ResolvedAppeals = 0,
                EfficiencyPercentage = 0
            };
        }

        return new EfficiencyReportDto
        {
            CouncilId = efficiency.CouncilId.GetValueOrDefault(),
            CouncilName = efficiency.CouncilName ?? council.Name,
            TotalAppeals = efficiency.TotalAppeals.GetValueOrDefault(),
            ResolvedAppeals = efficiency.ResolvedAppeals.GetValueOrDefault(),
            EfficiencyPercentage = efficiency.EfficiencyPercentage.GetValueOrDefault()
        };
    }

    public async Task<IEnumerable<EfficiencyReportDto>> GetAllCouncilsEfficiencyAsync()
    {
        var efficiencyReports = await _context.VCouncilEfficiencyReports.ToListAsync();

        return efficiencyReports.Select(r => new EfficiencyReportDto
        {
            CouncilId = r.CouncilId.GetValueOrDefault(),
            CouncilName = r.CouncilName ?? "Unknown",
            TotalAppeals = r.TotalAppeals.GetValueOrDefault(),
            ResolvedAppeals = r.ResolvedAppeals.GetValueOrDefault(),
            EfficiencyPercentage = r.EfficiencyPercentage.GetValueOrDefault()
        }).ToList();
    }
}

