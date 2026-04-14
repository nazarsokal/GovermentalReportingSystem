using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class Admin
{
    public Guid AdminId { get; set; }

    public Guid UserId { get; set; }

    public string AccessLevel { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
