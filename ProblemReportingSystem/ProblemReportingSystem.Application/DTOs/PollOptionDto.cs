namespace ProblemReportingSystem.Application.DTOs;

public class PollOptionDto
{
    public Guid OptionId { get; set; }

    public Guid PollId { get; set; }

    public string OptionText { get; set; } = null!;
}

public class CreatePollOptionDto
{
    public Guid PollId { get; set; }

    public string OptionText { get; set; } = null!;
}

public class UpdatePollOptionDto
{
    public Guid OptionId { get; set; }

    public string OptionText { get; set; } = null!;
}

public class PollOptionDetailsDto : PollOptionDto
{
    public int VoteCount { get; set; }
}

