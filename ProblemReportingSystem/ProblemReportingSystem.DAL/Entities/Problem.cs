using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class Problem
{
    public Guid ProblemId { get; set; }

    public Guid AddressId { get; set; }

    public Guid CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<Appeal> Appeals { get; set; } = new List<Appeal>();

    public virtual ProblemCategory Category { get; set; } = null!;

    public virtual ICollection<ProblemPhoto> ProblemPhotos { get; set; } = new List<ProblemPhoto>();
}
