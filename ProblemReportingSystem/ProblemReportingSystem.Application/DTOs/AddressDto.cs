namespace ProblemReportingSystem.Application.DTOs;

public class AddressDto
{
    public Guid AddressId { get; set; }
    
    public string City { get; set; } = string.Empty;
    
    public string Street { get; set; } = string.Empty;
    
    public string BuildingNumber { get; set; } = string.Empty;
    
    public string District { get; set; } = string.Empty;
    
    public string Oblast { get; set; } = string.Empty;
    
    public string Country { get; set; } = string.Empty;
    
    public string Postcode { get; set; } = string.Empty;
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

public class ParsedAddressDto
{
    public string City { get; set; } = string.Empty;
    
    public string Street { get; set; } = string.Empty;
    
    public string BuildingNumber { get; set; } = string.Empty;
    
    public string District { get; set; } = string.Empty;
    
    public string Oblast { get; set; } = string.Empty;
    
    public string Country { get; set; } = string.Empty;
    
    public string Postcode { get; set; } = string.Empty;
    
    public string FullAddress { get; set; } = string.Empty;
}

public class DistrictCoordinatesDto
{
    public string District { get; set; } = string.Empty;
    
    public decimal Latitude { get; set; }
    
    public decimal Longitude { get; set; }
}
