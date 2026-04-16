namespace ProblemReportingSystem.Application.DTOs;

public class StatisticsDto
{
    public long TotalAppeals { get; set; }

    public long PendingAppeals { get; set; }

    public long ResolvedAppeals { get; set; }

    public decimal AverageResolutionDays { get; set; }
}

public class EfficiencyReportDto
{
    public Guid CouncilId { get; set; }

    public string CouncilName { get; set; } = null!;

    public long TotalAppeals { get; set; }

    public long ResolvedAppeals { get; set; }

    public decimal EfficiencyPercentage { get; set; }
}

public class AppealStatisticsDto
{
    public int TotalAppeals { get; set; }
    public int ResolvedAppeals { get; set; }
    public int PendingAppeals { get; set; }
    public int AssignedAppeals { get; set; }
    public int UnassignedAppeals { get; set; }
    public double ResolutionRate { get; set; } // Percentage
    public int AverageResolutionDays { get; set; }
    public DateTime? LastUpdated { get; set; }
}

public class AppealTrendDto
{
    public DateTime Date { get; set; }
    public int CreatedCount { get; set; }
    public int ResolvedCount { get; set; }
    public int PendingCount { get; set; }
}
