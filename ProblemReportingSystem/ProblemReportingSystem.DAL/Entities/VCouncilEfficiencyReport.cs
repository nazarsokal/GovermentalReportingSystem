using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class VCouncilEfficiencyReport
{
    public Guid? CouncilId { get; set; }

    public string? CouncilName { get; set; }

    public long? TotalAppeals { get; set; }

    public long? ResolvedAppeals { get; set; }

    public decimal? EfficiencyPercentage { get; set; }
}
