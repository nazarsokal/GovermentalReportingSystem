using System.ComponentModel.DataAnnotations;

namespace ProblemReportingSystem.API.Contracts.Request;

public class RegisterCouncilEmployeeRequest
{
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Position { get; set; } = string.Empty;
}
