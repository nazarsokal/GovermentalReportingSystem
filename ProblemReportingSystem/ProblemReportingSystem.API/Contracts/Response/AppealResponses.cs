namespace ProblemReportingSystem.API.Contracts.Response;

public class AppealDetailResponse
{
    public Guid AppealId { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? AssignedEmployeeId { get; set; }
    public string? AssignedEmployeeName { get; set; }
    public DetailProblemResponse? Problem { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
