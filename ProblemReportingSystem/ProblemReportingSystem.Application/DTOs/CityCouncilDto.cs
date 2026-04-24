namespace ProblemReportingSystem.Application.DTOs;

public class CityCouncilDto
{
    public Guid CouncilId { get; set; }

    public Guid? AddressId { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactEmail { get; set; }
}

public class CreateCityCouncilDto
{
    public Guid? AddressId { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactEmail { get; set; }
}

public class UpdateCityCouncilDto
{
    public Guid CouncilId { get; set; }

    public Guid? AddressId { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactEmail { get; set; }
}

public class CityCouncilDetailsDto : CityCouncilDto
{
    public AddressDto? Address { get; set; }

    public ICollection<CouncilEmployeeDto> CouncilEmployees { get; set; } = new List<CouncilEmployeeDto>();

    public ICollection<PollDto> Polls { get; set; } = new List<PollDto>();
}

/// <summary>
/// DTO for city council response that includes address information
/// </summary>
public class CityCouncilWithAddressDto
{
    public Guid CouncilId { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactEmail { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public string? Oblast { get; set; }
}
