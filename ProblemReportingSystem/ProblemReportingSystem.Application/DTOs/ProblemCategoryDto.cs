namespace ProblemReportingSystem.Application.DTOs;

public class ProblemCategoryDto
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? IconUrl { get; set; }
}

public class CreateProblemCategoryDto
{
    public string Name { get; set; } = null!;

    public string? IconUrl { get; set; }
}

public class UpdateProblemCategoryDto
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? IconUrl { get; set; }
}

