namespace ProblemReportingSystem.API.Contracts.Response;

public class EfficiencyResponse
{
    public Guid CouncilId { get; set; }

    public string CouncilName { get; set; } = null!;

    public long TotalAppeals { get; set; }

    public long ResolvedAppeals { get; set; }

    public decimal EfficiencyPercentage { get; set; }

    public List<DailyEfficiencyPoint> DailyData { get; set; } = new List<DailyEfficiencyPoint>();
}

public class DailyEfficiencyPoint
{
    public DateTime Date { get; set; }

    public int ResolvedCount { get; set; }

    public decimal EfficiencyPercentage { get; set; }
}

