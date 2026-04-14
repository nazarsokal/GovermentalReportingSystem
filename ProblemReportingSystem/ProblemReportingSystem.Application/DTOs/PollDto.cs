namespace ProblemReportingSystem.Application.DTOs;

public class PollDto
{
    public Guid PollId { get; set; }

    public Guid CouncilId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool? IsActive { get; set; }
}

public class CreatePollDto
{
    public Guid CouncilId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}

public class UpdatePollDto
{
    public Guid PollId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }
}

public class PollDetailsDto : PollDto
{
    public CityCouncilDto? Council { get; set; }

    public ICollection<PollOptionDetailsDto> PollOptions { get; set; } = new List<PollOptionDetailsDto>();
}

