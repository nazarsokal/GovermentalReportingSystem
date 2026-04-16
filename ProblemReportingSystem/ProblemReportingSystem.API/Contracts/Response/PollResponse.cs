namespace ProblemReportingSystem.API.Contracts.Response;

public class PollResponse
{
    public Guid PollId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool? IsActive { get; set; }

    public List<PollOptionResponse> Options { get; set; } = new List<PollOptionResponse>();
}

public class PollOptionResponse
{
    public Guid OptionId { get; set; }

    public string OptionText { get; set; } = null!;

    public int VoteCount { get; set; }

    public decimal VotePercentage { get; set; }
}

