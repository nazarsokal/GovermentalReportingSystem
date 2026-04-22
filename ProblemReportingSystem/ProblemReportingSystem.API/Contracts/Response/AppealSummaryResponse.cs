namespace ProblemReportingSystem.API.Contracts.Response;

public class AppealSummaryResponse
{
    public Guid AppealId { get; set; }

    public DateTime? DatePublished { get; set; }

    public string ProblemName { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Description { get; set; } = null!;
}

