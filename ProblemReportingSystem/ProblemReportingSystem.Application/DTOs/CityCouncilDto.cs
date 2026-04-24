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
    public string Name { get; set; } = null!;

    public string? ContactEmail { get; set; }

    /// <summary>
    /// The ID of the user who will be the head of this city council
    /// </summary>
    public Guid HeadUserId { get; set; }

    /// <summary>
    /// Postal code
    /// </summary>
    public string? Postcode { get; set; }

    /// <summary>
    /// Latitude coordinate (optional - if not provided, geocoding service will be used)
    /// </summary>
    public decimal Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate (optional - if not provided, geocoding service will be used)
    /// </summary>
    public decimal Longitude { get; set; }
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
