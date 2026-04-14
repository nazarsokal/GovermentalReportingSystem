using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class ProblemCategory
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? IconUrl { get; set; }

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
