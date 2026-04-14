namespace ProblemReportingSystem.Application.DTOs;

public class ProblemPhotoDto
{
    public Guid PhotoId { get; set; }

    public Guid ProblemId { get; set; }

    public string PhotoUrl { get; set; } = null!;
}

public class CreateProblemPhotoDto
{
    public Guid ProblemId { get; set; }

    public string PhotoUrl { get; set; } = null!;
}

public class UpdateProblemPhotoDto
{
    public Guid PhotoId { get; set; }

    public string PhotoUrl { get; set; } = null!;
}

