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

