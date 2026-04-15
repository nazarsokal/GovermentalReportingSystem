namespace ProblemReportingSystem.API.Contracts.Request;

public class CreateProblemRequest
{
    public Guid CategoryId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public List<IFormFile>? Photos { get; set; }
}