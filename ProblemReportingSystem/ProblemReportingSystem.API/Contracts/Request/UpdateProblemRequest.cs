namespace ProblemReportingSystem.API.Contracts.Request;

/// <summary>
/// Request contract for updating an existing problem.
/// </summary>
public class UpdateProblemRequest
{
    /// <summary>
    /// The ID of the problem to update.
    /// </summary>
    public Guid ProblemId { get; set; }

    /// <summary>
    /// The updated title of the problem.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// The updated description of the problem.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// The updated status of the problem (e.g., "Open", "In Progress", "Resolved").
    /// </summary>
    public string? Status { get; set; }
}

