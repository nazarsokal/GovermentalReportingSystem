namespace ProblemReportingSystem.API.Contracts.Response;

public class SummaryProblemResponse
{
    public Guid ProblemId { get; set; }
    
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; }

    public string City { get; set; } = string.Empty;

    public string Oblast { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;
}

