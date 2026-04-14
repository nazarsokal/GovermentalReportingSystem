namespace ProblemReportingSystem.Application.DTOs;

public class CouncilEmployeeDto
{
    public Guid EmployeeId { get; set; }

    public Guid UserId { get; set; }

    public Guid CouncilId { get; set; }

    public string? Position { get; set; }
}

public class CreateCouncilEmployeeDto
{
    public Guid UserId { get; set; }

    public Guid CouncilId { get; set; }

    public string? Position { get; set; }
}

public class UpdateCouncilEmployeeDto
{
    public Guid EmployeeId { get; set; }

    public string? Position { get; set; }
}

public class CouncilEmployeeDetailsDto : CouncilEmployeeDto
{
    public UserDto? User { get; set; }

    public CityCouncilDto? Council { get; set; }
}

