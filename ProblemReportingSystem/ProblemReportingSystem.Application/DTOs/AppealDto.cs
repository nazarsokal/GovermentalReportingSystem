namespace ProblemReportingSystem.Application.DTOs;

public class AppealDto
{
    public Guid AppealId { get; set; }

    public Guid UserId { get; set; }

    public string? UserFullName { get; set; }

    public string? AssignedEmployeeFullName { get; set; }

    public CreateProblemDto ProblemDto { get; set; }

    public Guid? AssignedEmployeeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class CreateAppealDto
{
    public Guid UserId { get; set; }

    public Guid ProblemId { get; set; }

    public Guid? AssignedEmployeeId { get; set; }
}

public class UpdateAppealDto
{
    public Guid AppealId { get; set; }

    public Guid? AssignedEmployeeId { get; set; }
}

public class AppealDetailsDto : AppealDto
{
    public UserDto? User { get; set; }

    public ProblemDto? Problem { get; set; }

    public CouncilEmployeeDto? AssignedEmployee { get; set; }
}
