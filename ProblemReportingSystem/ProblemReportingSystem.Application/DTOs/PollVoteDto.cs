namespace ProblemReportingSystem.Application.DTOs;

public class PollVoteDto
{
    public Guid VoteId { get; set; }

    public Guid UserId { get; set; }

    public Guid OptionId { get; set; }

    public DateTime? VotedAt { get; set; }
}

public class CreatePollVoteDto
{
    public Guid UserId { get; set; }

    public Guid OptionId { get; set; }
}

public class PollVoteDetailsDto : PollVoteDto
{
    public UserDto? User { get; set; }

    public PollOptionDto? Option { get; set; }
}

