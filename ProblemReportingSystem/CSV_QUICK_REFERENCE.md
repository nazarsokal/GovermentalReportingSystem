# CSV Upload Quick Reference

## Installation
The CsvHelper NuGet package has been added to the project:
```
<PackageReference Include="CsvHelper" Version="31.0.3" />
```

## Files Created/Modified
- ✅ `AppealController.cs` - Added `UploadAppealsFromCsv` endpoint
- ✅ `AppealCsvMap.cs` - CSV record mapping classes
- ✅ `IAppealService.cs` - Added `CreateAppealsFromCsvAsync` method
- ✅ `AppealService.cs` - Implemented bulk create method
- ✅ `UploadAppealsFromCsvRequest.cs` - Request contract
- ✅ `UploadAppealsFromCsvResponse.cs` - Response contracts
- ✅ `ProblemReportingSystem.API.csproj` - Added CsvHelper dependency
- ✅ `CSV_UPLOAD_GUIDE.md` - User documentation
- ✅ `sample_appeals.csv` - Sample data file

## Endpoint Info
- **Route:** `POST /api/appeal/upload-csv`
- **Auth:** Required (JWT)
- **Content-Type:** multipart/form-data

## Quick Test

### Using Postman
1. Create new POST request to `http://localhost:5000/api/appeal/upload-csv`
2. Go to Authorization tab → select "Bearer Token" → paste JWT
3. Go to Body tab → select "form-data"
4. Add key "CsvFile" with type "File" → select CSV file
5. Add key "DefaultUserId" (optional) with UUID value
6. Send request

### Using curl
```bash
curl -X POST \
  http://localhost:5000/api/appeal/upload-csv \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "CsvFile=@appeals.csv"
```

## CSV Format
```
CategoryId,Title,Description,Status,City,Street,BuildingNumber,Latitude,Longitude
550e8400-e29b-41d4-a716-446655440000,Issue Title,Issue Description,Pending,City Name,Street Name,123,39.7817,-89.2500
```

## Response Summary
```json
{
  "success": true/false,
  "message": "Status message",
  "totalRecords": 10,
  "successfullyCreated": 9,
  "failedRecords": 1,
  "errors": [
    {
      "rowNumber": 5,
      "errorMessage": "Error description",
      "rowData": {...}
    }
  ]
}
```

## Key Points
- CategoryId and Title are required (cannot be empty)
- Latitude/Longitude cannot be 0
- Invalid rows are skipped but processing continues
- Maximum 100 errors returned (to keep response size manageable)
- Uses authenticated user ID if DefaultUserId not provided
- Each appeal creation follows all business logic and validation

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Only CSV files are allowed" | Ensure file has .csv extension |
| "CSV file contains no data records" | Check file has header row + data rows |
| "CategoryId is required and must be a valid GUID" | Verify all CategoryIds are valid UUIDs |
| "Title and Description are required" | Ensure these columns are not empty |
| "Latitude and Longitude must be provided" | Check coordinates are present and non-zero |
| 401 Unauthorized | Verify JWT token is valid and included in Authorization header |

## Performance Notes
- Processes records sequentially (not parallel)
- Good for batches up to ~1000 records
- For larger imports, consider splitting into multiple files
- Each record creation includes database operations

## Related Documentation
- See `CSV_UPLOAD_GUIDE.md` for detailed usage documentation
- See `CSV_IMPLEMENTATION_SUMMARY.md` for technical implementation details
- Use `sample_appeals.csv` as template for your data

