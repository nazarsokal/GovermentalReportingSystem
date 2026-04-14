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
}

public class CreateProblemDto
{
    public Guid AddressId { get; set; }

    public Guid CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; }
}

public class UpdateProblemDto
{
    public Guid ProblemId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; }
}

public class ProblemDetailsDto : ProblemDto
{
    public AddressDto? Address { get; set; }

    public ProblemCategoryDto? Category { get; set; }

    public ICollection<ProblemPhotoDto> ProblemPhotos { get; set; } = new List<ProblemPhotoDto>();
}

