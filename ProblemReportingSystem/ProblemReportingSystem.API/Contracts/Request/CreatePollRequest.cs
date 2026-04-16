namespace ProblemReportingSystem.API.Contracts.Request;

public class CreatePollRequest
{
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public List<string> Options { get; set; } = new List<string>();
}

