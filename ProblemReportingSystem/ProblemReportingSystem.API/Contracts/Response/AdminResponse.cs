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
