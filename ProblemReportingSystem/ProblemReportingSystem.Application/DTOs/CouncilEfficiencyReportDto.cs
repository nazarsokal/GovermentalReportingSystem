namespace ProblemReportingSystem.Application.DTOs;

public class CouncilEfficiencyReportDto
{
    public Guid? CouncilId { get; set; }

    public string? CouncilName { get; set; }

    public long? TotalAppeals { get; set; }

    public long? ResolvedAppeals { get; set; }

    public decimal? EfficiencyPercentage { get; set; }
}

