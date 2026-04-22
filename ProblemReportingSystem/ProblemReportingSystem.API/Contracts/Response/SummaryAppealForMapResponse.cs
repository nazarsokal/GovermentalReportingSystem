namespace ProblemReportingSystem.API.Contracts.Response;

public class SummaryAppealForMapResponse
{
    public Guid AppealId { get; set; }
    
    public double Latitude { get; set; }
    
    public double Longitude { get; set; }

    public string CategoryIconUrl { get; set; }
}