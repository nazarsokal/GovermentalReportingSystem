namespace ProblemReportingSystem.API.Contracts.Request;

public class UploadAppealsFromCsvRequest
{
    public required IFormFile CsvFile { get; set; }
    
    public Guid? DefaultUserId { get; set; }
}

