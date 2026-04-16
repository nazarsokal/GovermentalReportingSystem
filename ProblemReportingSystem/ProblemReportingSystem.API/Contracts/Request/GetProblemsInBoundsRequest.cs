namespace ProblemReportingSystem.API.Contracts.Request;

/// <summary>
/// Request contract for retrieving problems within geographic bounds.
/// Useful for map-based queries where you need to find problems in a specific area.
/// </summary>
public class GetProblemsInBoundsRequest
{
    /// <summary>
    /// Minimum latitude of the geographic bounds.
    /// </summary>
    public decimal MinLatitude { get; set; }

    /// <summary>
    /// Maximum latitude of the geographic bounds.
    /// </summary>
    public decimal MaxLatitude { get; set; }

    /// <summary>
    /// Minimum longitude of the geographic bounds.
    /// </summary>
    public decimal MinLongitude { get; set; }

    /// <summary>
    /// Maximum longitude of the geographic bounds.
    /// </summary>
    public decimal MaxLongitude { get; set; }
}

