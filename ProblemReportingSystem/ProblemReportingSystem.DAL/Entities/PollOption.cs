using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class PollOption
{
    public Guid OptionId { get; set; }

    public Guid PollId { get; set; }

    public string OptionText { get; set; } = null!;

    public virtual Poll Poll { get; set; } = null!;

    public virtual ICollection<PollVote> PollVotes { get; set; } = new List<PollVote>();
}
