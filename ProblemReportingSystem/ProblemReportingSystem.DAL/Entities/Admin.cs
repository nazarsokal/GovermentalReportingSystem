using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class Admin : IUserEntity
{
    public Guid AdminId { get; set; }

    public string AccessLevel { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
