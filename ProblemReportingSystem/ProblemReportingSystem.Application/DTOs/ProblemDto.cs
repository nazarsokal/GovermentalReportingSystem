namespace ProblemReportingSystem.Application.DTOs;

public class ProblemDto
{
    public Guid ProblemId { get; set; }

    public Guid AddressId { get; set; }

    public Guid CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }
    
    public AddressDto Address { get; set; } = null!;
    
    public List<ProblemPhotoDto> Photos { get; set; } = new List<ProblemPhotoDto>();
}

public class CreateProblemDto
{
    public Guid UserId { get; set; }

    public Guid CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; }
    
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? District { get; set; }
    public string? Oblast { get; set; }
    public string? Postcode { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public List<CreateProblemPhotoDto>? Photos { get; set; }
}

public class UpdateProblemDto
{
    public Guid ProblemId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; }
}
