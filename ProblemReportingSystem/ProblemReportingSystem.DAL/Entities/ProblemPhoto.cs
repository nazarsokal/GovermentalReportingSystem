using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class ProblemPhoto
{
    public Guid PhotoId { get; set; }

    public Guid ProblemId { get; set; }

    public string PhotoUrl { get; set; } = null!;

    public virtual Problem Problem { get; set; } = null!;
}
