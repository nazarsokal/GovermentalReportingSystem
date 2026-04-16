namespace ProblemReportingSystem.API.Contracts.Response;

public class AppealResponse
{
    public Guid AppealId { get; set; }

    public Guid ProblemId { get; set; }

    public Guid? AssignedEmployeeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public SummaryProblemResponse? Problem { get; set; }

    public string? AssignedEmployeeName { get; set; }
}

