namespace ProblemReportingSystem.API.Contracts.Response;

public class KPIResponse
{
    public long TotalAppeals { get; set; }

    public long PendingAppeals { get; set; }

    public long ResolvedAppeals { get; set; }

    public decimal AverageResolutionDays { get; set; }
}

