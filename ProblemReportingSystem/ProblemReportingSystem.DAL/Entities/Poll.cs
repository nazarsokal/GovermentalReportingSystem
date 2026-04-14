using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class Poll
{
    public Guid PollId { get; set; }

    public Guid CouncilId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual CityCouncil Council { get; set; } = null!;

    public virtual ICollection<PollOption> PollOptions { get; set; } = new List<PollOption>();
}
