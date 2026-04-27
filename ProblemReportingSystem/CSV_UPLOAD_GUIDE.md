# CSV Appeal Upload Feature

## Overview
The CSV Upload feature allows bulk importing of appeals from a CSV file. This is useful for migrating existing problem reports or performing batch imports.

## Endpoint

**POST** `/api/appeal/upload-csv`

## Request Format

### Form Parameters
- `CsvFile` (required): The CSV file to upload (multipart/form-data)
- `DefaultUserId` (optional): GUID of the default user to assign appeals. If not provided, the authenticated user's ID will be used.

### CSV File Format

The CSV file should have the following columns (header row required):

| Column Name | Type | Required | Description |
|------------|------|----------|-------------|
| CategoryId | GUID | Yes | The ID of the problem category |
| Title | String | Yes | Brief title of the appeal/problem |
| Description | String | Yes | Detailed description of the problem |
| Status | String | No | Appeal status (default: "Pending") |
| City | String | No | City name |
| Street | String | No | Street name |
| BuildingNumber | String | No | Building/house number |
| Latitude | Decimal | Yes | Geographic latitude coordinate |
| Longitude | Decimal | Yes | Geographic longitude coordinate |

### Sample CSV File

```csv
CategoryId,Title,Description,Status,City,Street,BuildingNumber,Latitude,Longitude
550e8400-e29b-41d4-a716-446655440000,Pothole on Main Street,Large pothole affecting traffic,Pending,Springfield,Main St,123,39.7817,-89.2500
550e8400-e29b-41d4-a716-446655440001,Broken Street Light,Street light at intersection not working,,Springfield,Oak Ave,45,39.7819,-89.2501
550e8400-e29b-41d4-a716-446655440000,Trash Collection Issue,Garbage not collected for 3 weeks,In Progress,Springfield,Elm Road,89,39.7815,-89.2502
```

## Response Format

### Success Response (HTTP 200)

```json
{
  "success": true,
  "message": "CSV processing complete: 3 appeals created successfully",
  "totalRecords": 3,
  "successfullyCreated": 3,
  "failedRecords": 0,
  "errors": []
}
```

### Partial Success Response (HTTP 200)

```json
{
  "success": false,
  "message": "CSV processing complete: 2 appeals created successfully, 1 failed",
  "totalRecords": 3,
  "successfullyCreated": 2,
  "failedRecords": 1,
  "errors": [
    {
      "rowNumber": 3,
      "errorMessage": "Latitude and Longitude must be provided",
      "rowData": {
        "CategoryId": "550e8400-e29b-41d4-a716-446655440000",
        "Title": "Test Appeal",
        "Description": "Test Description",
        "Status": "Pending",
        "City": "Springfield",
        "Street": "Test St",
        "BuildingNumber": "123",
        "Latitude": "0",
        "Longitude": "0"
      }
    }
  ]
}
```

### Error Response (HTTP 400)

```json
{
  "success": false,
  "message": "CSV file contains no data records"
}
```

## Validation Rules

The following validations are performed:

1. **File Validation**
   - File must have .csv extension
   - File cannot be empty
   
2. **Row Validation** (for each row)
   - `CategoryId`: Required, must be a valid GUID
   - `Title`: Required, cannot be empty
   - `Description`: Required, cannot be empty
   - `Latitude`: Required, must be non-zero and valid decimal
   - `Longitude`: Required, must be non-zero and valid decimal
   
3. **Optional Fields**
   - `Status`: Defaults to "Pending" if not provided
   - `City`, `Street`, `BuildingNumber`: Optional, can be empty

## Error Handling

The endpoint returns a mix of validation errors and creation errors in the response. Each error includes:
- `rowNumber`: The CSV row number where the error occurred
- `errorMessage`: Description of the error
- `rowData`: The actual row data that caused the error (for validation errors)

Invalid rows are skipped and reported in the response, but valid rows continue to be processed.

## Usage Example

### Using curl

```bash
curl -X POST \
  http://localhost:5000/api/appeal/upload-csv \
  -H 'Authorization: Bearer YOUR_JWT_TOKEN' \
  -F 'CsvFile=@appeals.csv' \
  -F 'DefaultUserId=550e8400-e29b-41d4-a716-446655440000'
```

### Using PowerShell

```powershell
$headers = @{
    "Authorization" = "Bearer YOUR_JWT_TOKEN"
}

$form = @{
    CsvFile = Get-Item -Path "C:\appeals.csv"
    DefaultUserId = "550e8400-e29b-41d4-a716-446655440000"
}

Invoke-RestMethod -Uri "http://localhost:5000/api/appeal/upload-csv" `
    -Method Post `
    -Headers $headers `
    -Form $form
```

### Using JavaScript/Fetch

```javascript
const formData = new FormData();
formData.append('CsvFile', fileInputElement.files[0]);
formData.append('DefaultUserId', '550e8400-e29b-41d4-a716-446655440000');

const response = await fetch('http://localhost:5000/api/appeal/upload-csv', {
    method: 'POST',
    headers: {
        'Authorization': 'Bearer YOUR_JWT_TOKEN'
    },
    body: formData
});

const result = await response.json();
console.log(result);
```

## Best Practices

1. **Data Validation**: Validate your data before uploading to ensure all required fields are present
2. **Batch Size**: For large datasets, consider splitting into multiple files to manage memory usage
3. **Category Validation**: Ensure all CategoryIds exist in your system before importing
4. **Coordinate Accuracy**: Verify that latitude and longitude values are accurate and valid
5. **Error Review**: Review the error list in the response to identify and fix problematic rows
6. **Retry Strategy**: Implement exponential backoff when retrying failed uploads

## Limitations

- Errors are limited to the first 100 entries in the response to keep response size manageable
- The endpoint processes records sequentially, so large files may take time
- Each appeal creation follows the same business logic as single appeal creation

## Notes

- If `DefaultUserId` is not provided, the endpoint uses the authenticated user's ID from the JWT token
- The upload endpoint requires authentication (JWT token in Authorization header)
- All timestamps (CreatedAt, UpdatedAt) are automatically set by the system
- Photos cannot be included in CSV uploads; they must be added separately after creation

