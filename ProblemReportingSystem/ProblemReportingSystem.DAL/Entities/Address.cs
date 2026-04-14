using System;
using System.Collections.Generic;

namespace ProblemReportingSystem.DAL.Entities;

public partial class Address
{
    public Guid AddressId { get; set; }

    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string BuildingNumber { get; set; } = null!;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public virtual ICollection<CityCouncil> CityCouncils { get; set; } = new List<CityCouncil>();

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
