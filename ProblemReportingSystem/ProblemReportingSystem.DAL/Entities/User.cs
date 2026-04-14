using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class User
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? GoogleAuthId { get; set; }

    public bool? IsActive { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<Appeal> Appeals { get; set; } = new List<Appeal>();

    public virtual CouncilEmployee? CouncilEmployee { get; set; }

    public virtual ICollection<PollVote> PollVotes { get; set; } = new List<PollVote>();
}
