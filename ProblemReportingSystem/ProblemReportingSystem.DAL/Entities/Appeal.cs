using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class Appeal
{
    public Guid AppealId { get; set; }

    public Guid UserId { get; set; }

    public Guid ProblemId { get; set; }

    public Guid? AssignedEmployeeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual CouncilEmployee? AssignedEmployee { get; set; }

    public virtual Problem Problem { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
