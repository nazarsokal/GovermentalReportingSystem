namespace ProblemReportingSystem.Application.DTOs;

public class AddressDto
{
    public Guid AddressId { get; set; }
    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string BuildingNumber { get; set; } = null!;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}

public class CreateAddressDto
{
    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string BuildingNumber { get; set; } = null!;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}

public class UpdateAddressDto
{
    public Guid AddressId { get; set; }

    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string BuildingNumber { get; set; } = null!;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}





