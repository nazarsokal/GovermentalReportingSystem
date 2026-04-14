using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class PollVote
{
    public Guid VoteId { get; set; }

    public Guid UserId { get; set; }

    public Guid OptionId { get; set; }

    public DateTime? VotedAt { get; set; }

    public virtual PollOption Option { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
