# CSV Appeal Upload Implementation Summary

## Overview
A complete CSV upload feature has been implemented for bulk importing appeals into the system.

## Components Created/Modified

### 1. **AppealController.cs** (Modified)
- Added `UploadAppealsFromCsv()` endpoint at `POST /api/appeal/upload-csv`
- Handles CSV file validation and parsing
- Provides detailed error reporting for validation and creation failures
- Extracts user ID from JWT claims if not provided in request

### 2. **AppealCsvMap.cs** (New)
- Created `AppealCsvRecord` class to represent CSV row data
- Created `AppealCsvMap` class  for CsvHelper mapping
- Defines mapping between CSV columns and object properties
- Handles optional and required field mapping

### 3. **IAppealService.cs** (Modified)
- Added `CreateAppealsFromCsvAsync()` method signature
- Returns detailed results for each appeal creation attempt

### 4. **AppealService.cs** (Modified)
- Implemented `CreateAppealsFromCsvAsync()` method
- Processes appeals sequentially with error handling
- Returns success/failure status for each appeal

### 5. **UploadAppealsFromCsvRequest.cs** (New)
- Request contract for CSV file upload
- Includes optional DefaultUserId parameter

### 6. **UploadAppealsFromCsvResponse.cs** (New)
- Response contract with detailed upload results
- Includes error details with row numbers and error messages
- Contains summary statistics (total, successful, failed counts)

### 7. **ProblemReportingSystem.API.csproj** (Modified)
- Added `CsvHelper v31.0.3` NuGet package dependency

### 8. **CSV_UPLOAD_GUIDE.md** (New)
- Comprehensive documentation for using the CSV upload feature
- Includes request/response formats, validation rules, usage examples

### 9. **sample_appeals.csv** (New)
- Sample CSV file with test data for reference

## API Endpoint Details

### Endpoint: POST /api/appeal/upload-csv

**Request:**
```
Form Data:
- CsvFile: IFormFile (required, .csv file)
- DefaultUserId: Guid (optional)
```

**Response (Success - HTTP 200):**
```json
{
  "success": true,
  "message": "CSV processing complete: X appeals created successfully",
  "totalRecords": X,
  "successfullyCreated": X,
  "failedRecords": 0,
  "errors": []
}
```

**Response (Partial Success - HTTP 200):**
```json
{
  "success": false,
  "message": "CSV processing complete: X appeals created successfully, Y failed",
  "totalRecords": X,
  "successfullyCreated": X,
  "failedRecords": Y,
  "errors": [
    {
      "rowNumber": 5,
      "errorMessage": "Error description",
      "rowData": {...}
    }
  ]
}
```

**Response (Error - HTTP 400):**
```json
{
  "success": false,
  "message": "Error description"
}
```

## CSV File Format

Required columns:
- `CategoryId` (GUID) - Required
- `Title` (String) - Required
- `Description` (String) - Required
- `Latitude` (Decimal) - Required
- `Longitude` (Decimal) - Required

Optional columns:
- `Status` (String) - Defaults to "Pending"
- `City` (String)
- `Street` (String)
- `BuildingNumber` (String)

## Validation Features

1. **File Validation:**
   - Must be .csv file
   - Cannot be empty
   - Must contain at least one data row

2. **Data Validation:**
   - CategoryId must be valid GUID
   - Title and Description required
   - Coordinates must be provided and non-zero
   - Proper error reporting with row numbers

3. **Creation Validation:**
   - Each appeal goes through the same business logic as single creation
   - Problems are created via ProblemService
   - Addresses are created as needed

## Features

✅ Bulk import of appeals from CSV
✅ Detailed error reporting (row numbers + error messages)
✅ Partial success handling (process what's valid)
✅ JWT authentication support
✅ Configurable default user ID
✅ Input validation with helpful error messages
✅ Logging for monitoring and debugging
✅ Response size management (errors limited to 100)

## Error Handling

- Invalid file format → HTTP 400
- No valid records → HTTP 400
- Partial success → HTTP 200 (with error details)
- Server errors → HTTP 500

Each failed row includes:
- Row number (for easy identification)
- Error message (reason for failure)
- Row data (for reference and debugging)

## Integration Notes

- Uses existing `CreateAppealAsync` internally
- Fully integrated with AutoMapper
- Works with existing ProblemService
- Supports all existing business logic and validation
- Compatible with database schema and repositories

## Future Enhancements

Possible future improvements:
- Async batch processing for very large files
- Progress tracking for long-running imports
- Photo attachment support
- Employee assignment from CSV
- Custom address mapping
- Database transaction rollback on complete failure option

## Testing Recommendations

1. Test with valid complete data
2. Test with missing optional fields
3. Test with invalid GUIDs
4. Test with missing coordinates
5. Test with empty file
6. Test with wrong file extension
7. Test with file size limits
8. Test partial success scenarios
9. Test without authentication
10. Test with default user ID vs. JWT user ID

## Security Considerations

- Requires JWT authentication
- Uses provided user ID or falls back to authenticated user
- File size should be validated (add max size limit if needed)
- SQL injection prevention via parameterized queries (handled by EF Core)
- Input validation on all fields

