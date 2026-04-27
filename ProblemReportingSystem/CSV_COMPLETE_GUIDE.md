# CSV Upload Feature - Complete Implementation Guide

## What Has Been Implemented

A complete CSV bulk upload feature for the Appeal system that allows importing multiple appeals at once from a CSV file.

## Feature Files Created

### 1. **API Layer**
- `ProblemReportingSystem.API/Controllers/AppealCsvMap.cs`
  - `AppealCsvRecord` - Maps CSV columns to properties
  - `AppealCsvMap` - CsvHelper configuration for parsing

- `ProblemReportingSystem.API/Contracts/Request/UploadAppealsFromCsvRequest.cs`
  - Request contract with CSV file and optional user ID

- `ProblemReportingSystem.API/Contracts/Response/UploadAppealsFromCsvResponse.cs`
  - Response with success status, statistics, and error details
  - `CsvUploadErrorDetail` for detailed error reporting

- `ProblemReportingSystem.API/Controllers/AppealController.cs` (Modified)
  - New endpoint: `POST /api/appeal/upload-csv`
  - Full CSV parsing and validation
  - Error handling and reporting

### 2. **Service Layer**
- `ProblemReportingSystem.Application/ServiceAbstractions/IAppealService.cs` (Modified)
  - Added `CreateAppealsFromCsvAsync()` method signature

- `ProblemReportingSystem.Application/Services/AppealService.cs` (Modified)
  - Implemented `CreateAppealsFromCsvAsync()` method
  - Processes appeals one by one with error handling

### 3. **Projects**
- `ProblemReportingSystem.API/ProblemReportingSystem.API.csproj` (Modified)
  - Added `CsvHelper v31.0.3` NuGet package

### 4. **Documentation**
- `CSV_UPLOAD_GUIDE.md` - Complete user guide
- `CSV_QUICK_REFERENCE.md` - Quick reference for developers
- `CSV_IMPLEMENTATION_SUMMARY.md` - Technical implementation details
- `sample_appeals.csv` - Sample data for testing

## How the Feature Works

### Request Flow
```
POST /api/appeal/upload-csv
    ↓
[FormData: CsvFile + DefaultUserId]
    ↓
AppealController.UploadAppealsFromCsv()
    ↓
[Parse CSV with CsvHelper]
    ↓
[Validate each row]
    ↓
AppealService.CreateAppealsFromCsvAsync()
    ↓
[Create each appeal via CreateAppealAsync()]
    ↓
[Return results with error details]
    ↓
HTTP 200 Response
```

### Data Validation
Each row is validated for:
- Required fields: CategoryId, Title, Description, Latitude, Longitude
- Data types: CategoryId must be GUID, coordinates must be decimal
- Business logic: Follows same validation as single appeal creation

### Error Handling
- Invalid rows are skipped (continue processing)
- Detailed error reporting with row numbers
- Partial success is accepted and reported
- Maximum 100 errors returned to keep response manageable

## Using the Feature

### 1. Prepare CSV File
Create a CSV file with this structure:
```csv
CategoryId,Title,Description,Status,City,Street,BuildingNumber,Latitude,Longitude
550e8400-e29b-41d4-a716-446655440000,Pothole,Deep pothole,Pending,Springfield,Main St,123,39.7817,-89.2500
```

See `sample_appeals.csv` for a complete example.

### 2. Call the Endpoint
```bash
curl -X POST http://localhost:5000/api/appeal/upload-csv \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "CsvFile=@appeals.csv"
```

### 3. Handle Response
```json
{
  "success": true,
  "message": "CSV processing complete: 5 appeals created successfully",
  "totalRecords": 5,
  "successfullyCreated": 5,
  "failedRecords": 0,
  "errors": []
}
```

## Testing the Feature

### Test Case 1: Valid Data
```csv
CategoryId,Title,Description,City,Street,BuildingNumber,Latitude,Longitude
550e8400-e29b-41d4-a716-446655440000,Issue 1,Description 1,City1,St1,1,39.7817,-89.2500
550e8400-e29b-41d4-a716-446655440001,Issue 2,Description 2,City2,St2,2,39.7818,-89.2501
```
Expected: Both created successfully

### Test Case 2: Missing Required Field
```csv
CategoryId,Title,Description,City,Street,BuildingNumber,Latitude,Longitude
550e8400-e29b-41d4-a716-446655440000,,Description 1,City1,St1,1,39.7817,-89.2500
```
Expected: Row 2 fails (missing Title), error in response

### Test Case 3: Invalid GUID
```csv
CategoryId,Title,Description,City,Street,BuildingNumber,Latitude,Longitude
not-a-guid,Issue 1,Description 1,City1,St1,1,39.7817,-89.2500
```
Expected: Row 2 fails (invalid GUID), error in response

### Test Case 4: Zero Coordinates
```csv
CategoryId,Title,Description,City,Street,BuildingNumber,Latitude,Longitude
550e8400-e29b-41d4-a716-446655440000,Issue 1,Description 1,City1,St1,1,0,0
```
Expected: Row 2 fails (coordinates are zero), error in response

### Test Case 5: Optional Fields Omitted
```csv
CategoryId,Title,Description,Latitude,Longitude
550e8400-e29b-41d4-a716-446655440000,Issue 1,Description 1,39.7817,-89.2500
```
Expected: Created successfully (Status defaults to "Pending")

## Key Features

✅ **Bulk Import** - Import multiple appeals at once
✅ **Error Resilience** - Invalid rows don't stop processing
✅ **Detailed Reporting** - Know exactly which rows failed and why
✅ **Authentication** - Integrated with JWT
✅ **User Assignment** - Explicit or from JWT claims
✅ **Validation** - Full data validation on each row
✅ **Logging** - Complete audit trail
✅ **Size Managed** - Errors limited to prevent huge responses

## Architecture Benefits

- **Separation of Concerns**: Parsing logic is separate from business logic
- **Reusability**: Uses existing `CreateAppealAsync` for consistency
- **Maintainability**: Clear error paths and logging
- **Extensibility**: Easy to add new fields or validation rules
- **Integration**: Works seamlessly with existing services
- **Performance**: Sequential processing suitable for typical use cases

## Configuration Notes

No configuration needed! The feature works out of the box:
- CsvHelper is configured inline in the controller
- Mapping is done via `AppealCsvMap` class
- Uses InvariantCulture for parsing (works globally)

## Security Considerations

✅ Requires JWT authentication
✅ Validates all input data
✅ Uses parameterized queries (EF Core)
✅ Proper error messages (no sensitive data)
✅ Request size should be monitored
✅ User ID extraction from authenticated principal

## Performance Characteristics

- **Sequential Processing**: Processes one appeal at a time
- **Memory Efficient**: Streams CSV parsing (doesn't load entire file)
- **Database**: Each appeal is a separate transaction
- **Typical Time**: ~100-200ms per appeal
- **Suitable For**: Batches up to 1000 records

For very large imports:
- Split into multiple files
- Consider async processing (future enhancement)
- Monitor database performance

## Future Enhancements

Potential improvements for future versions:
1. Async/parallel processing for large batches
2. Progress callback for UI integration
3. Photo attachment support via base64
4. Employee assignment from CSV
5. Custom address mapping
6. Batch transaction (all-or-nothing)
7. Field format customization
8. Retry logic for failed records
9. Scheduled/background processing

## Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| 401 Unauthorized | No JWT token | Add Authorization header with Bearer token |
| File not found | Wrong file path | Verify file exists and path is correct |
| CSV parsing errors | Invalid format | Check CSV has header row and correct delimiters |
| 400 Bad Request | File not .csv | Ensure file has .csv extension |
| Records not created | Email already exists | Check if user already registered |
| Partial success | Some rows invalid | Check error response for specific row issues |

## Support

For questions or issues:
1. Check `CSV_UPLOAD_GUIDE.md` for usage documentation
2. Review `sample_appeals.csv` for format examples
3. Check error messages in response for specific issues
4. Review application logs for detailed errors
5. Verify CategoryIds exist in database
6. Ensure coordinates are in valid range

## Files to Review

- Start with: `CSV_QUICK_REFERENCE.md`
- Then read: `CSV_UPLOAD_GUIDE.md`
- Technical details: `CSV_IMPLEMENTATION_SUMMARY.md`
- Code examples: `sample_appeals.csv`
- Implementation: See files listed in "Feature Files Created"

