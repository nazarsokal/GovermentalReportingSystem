namespace ProblemReportingSystem.API.Contracts.Response;

public class DetailProblemResponse
{
    public Guid ProblemId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public AddressResponse Address { get; set; } = null!;

    public ProblemCategoryResponse Category { get; set; } = null!;

    public List<ProblemPhotoResponse> Photos { get; set; } = new List<ProblemPhotoResponse>();
}

public class AddressResponse
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

public class ProblemCategoryResponse
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}

public class ProblemPhotoResponse
{
    public Guid PhotoId { get; set; }

    public byte[] ImageData { get; set; } = null!;

    public string ContentType { get; set; } = null!;
}

