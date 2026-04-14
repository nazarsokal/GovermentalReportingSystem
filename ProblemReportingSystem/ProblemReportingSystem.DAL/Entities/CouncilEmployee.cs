using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class CouncilEmployee
{
    public Guid EmployeeId { get; set; }

    public Guid UserId { get; set; }

    public Guid CouncilId { get; set; }

    public string? Position { get; set; }

    public virtual ICollection<Appeal> Appeals { get; set; } = new List<Appeal>();

    public virtual CityCouncil Council { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
