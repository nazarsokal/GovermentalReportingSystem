using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class CityCouncil
{
    public Guid CouncilId { get; set; }

    public Guid? AddressId { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactEmail { get; set; }

    public virtual Address? Address { get; set; }

    public virtual ICollection<CouncilEmployee> CouncilEmployees { get; set; } = new List<CouncilEmployee>();

    public virtual ICollection<Poll> Polls { get; set; } = new List<Poll>();
}
