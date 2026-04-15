using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProblemReportingSystem.DAL.Entities;

public partial class Address
{
    public Guid AddressId { get; set; }

    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string BuildingNumber { get; set; } = null!;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }
    
    [Column("district")] 
    [MaxLength(150)]
    public string? District { get; set; }

    [Column("oblast")] 
    [MaxLength(100)]
    public string? Oblast { get; set; }

    [Column("country")] 
    [MaxLength(100)]
    public string? Country { get; set; }

    [Column("postcode")] 
    [MaxLength(20)]
    public string? Postcode { get; set; }

    public virtual ICollection<CityCouncil> CityCouncils { get; set; } = new List<CityCouncil>();

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
