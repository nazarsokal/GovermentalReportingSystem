namespace ProblemReportingSystem.API.Contracts.Response;

public class UploadAppealsFromCsvResponse
{
    public bool Success { get; set; }
    
    public string Message { get; set; } = null!;
    
    public int TotalRecords { get; set; }
    
    public int SuccessfullyCreated { get; set; }
    
    public int FailedRecords { get; set; }
    
    public List<CsvUploadErrorDetail> Errors { get; set; } = new();
}

public class CsvUploadErrorDetail
{
    public int RowNumber { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public Dictionary<string, string>? RowData { get; set; }
}

