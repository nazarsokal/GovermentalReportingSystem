using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProblemReportingSystem.DAL.Entities;

public partial class User : IUserEntity
{
    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;
    
    [Column("password_hash")]
    public string? PasswordHash { get; set; } = null!;

    public string? GoogleAuthId { get; set; }
    
    [Column("address_id")]
    public Guid? AddressId { get; set; }
    
    [ForeignKey(nameof(AddressId))]
    public virtual Address? Address { get; set; }

    public bool? IsActive { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<Appeal> Appeals { get; set; } = new List<Appeal>();

    public virtual CouncilEmployee? CouncilEmployee { get; set; }

    public virtual ICollection<PollVote> PollVotes { get; set; } = new List<PollVote>();
}
