namespace ProblemReportingSystem.Application.DTOs;

public class ProblemPhotoDto
{
    public Guid PhotoId { get; set; }

    public Guid ProblemId { get; set; }

    public byte[] ImageData { get; set; } = null!;

    public string ContentType { get; set; } = null!;
}

public class CreateProblemPhotoDto
{
    public Guid ProblemId { get; set; }

    public byte[] ImageData { get; set; } = null!;

    public string ContentType { get; set; } = null!;
}

public class UpdateProblemPhotoDto
{
    public Guid PhotoId { get; set; }

    public byte[] ImageData { get; set; } = null!;

    public string ContentType { get; set; } = null!;
}
