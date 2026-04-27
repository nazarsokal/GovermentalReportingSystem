using CsvHelper.Configuration;

namespace ProblemReportingSystem.API.Controllers;

/// <summary>
/// CSV record mapping for appeals import.
/// </summary>
public class AppealCsvRecord
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Status { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

/// <summary>
/// CsvHelper class map for AppealCsvRecord.
/// Defines the mapping between CSV columns and object properties.
/// </summary>
public class AppealCsvMap : ClassMap<AppealCsvRecord>
{
    public AppealCsvMap()
    {
        Map(m => m.CategoryId).Name("CategoryId");
        Map(m => m.Title).Name("Title");
        Map(m => m.Description).Name("Description");
        Map(m => m.Status).Name("Status").Optional();
        Map(m => m.City).Name("City").Optional();
        Map(m => m.Street).Name("Street").Optional();
        Map(m => m.BuildingNumber).Name("BuildingNumber").Optional();
        Map(m => m.Latitude).Name("Latitude");
        Map(m => m.Longitude).Name("Longitude");
    }
}

