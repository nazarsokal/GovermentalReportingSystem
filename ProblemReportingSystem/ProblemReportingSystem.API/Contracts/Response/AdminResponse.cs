namespace ProblemReportingSystem.API.Contracts.Response;

public class ErrorResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = null!;

    public List<string>? Errors { get; set; }
}

public class CityCouncilLoadResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = null!;

    public int TotalCount { get; set; }

    public List<CityCouncilResponseDto> LoadedCouncils { get; set; } = new List<CityCouncilResponseDto>();
}

public class CityCouncilResponseDto
{
    public Guid CouncilId { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactEmail { get; set; }

    public Guid? AddressId { get; set; }
}

public class UsersListResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = null!;

    public int TotalCount { get; set; }

    public List<UserListItemDto> Users { get; set; } = new List<UserListItemDto>();
}

public class UserListItemDto
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool? IsActive { get; set; }

    public Guid? AddressId { get; set; }
}

public class CreateCityCouncilResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = null!;

    public Guid CouncilId { get; set; }

    public string CouncilName { get; set; } = null!;
}
