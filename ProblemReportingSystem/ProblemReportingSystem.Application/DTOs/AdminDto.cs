namespace ProblemReportingSystem.Application.DTOs;

public class AdminDto
{
    public Guid AdminId { get; set; }

    public Guid UserId { get; set; }

    public string AccessLevel { get; set; } = null!;
}

public class CreateAdminDto
{
    public Guid UserId { get; set; }

    public string AccessLevel { get; set; } = null!;
}

public class UpdateAdminDto
{
    public Guid AdminId { get; set; }

    public string AccessLevel { get; set; } = null!;
}

public class AdminDetailsDto : AdminDto
{
    public UserDto? User { get; set; }
}

