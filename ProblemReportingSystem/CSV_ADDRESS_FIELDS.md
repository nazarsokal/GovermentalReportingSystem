# CSV Address Fields Enhancement

**Date:** April 28, 2026

## Summary

Added support for importing additional address fields from CSV: **District**, **Oblast**, and **Postcode**. These fields are now optional in CSV imports and will be stored in the database.

---

## Updated CSV Format

### CSV Header (with new fields)
```csv
UserId,CategoryId,Title,Description,Status,City,Street,BuildingNumber,District,Oblast,Postcode,Latitude,Longitude
```

### Field Definitions

| Field | Type | Required | Max Length | Description |
|-------|------|----------|-----------|-------------|
| **UserId** | GUID | Optional | - | Appeal creator (defaults to DefaultUserId parameter if not provided) |
| **CategoryId** | GUID | ✅ Required | - | Problem category ID |
| **Title** | String | ✅ Required | - | Appeal title |
| **Description** | String | ✅ Required | - | Detailed description |
| **Status** | String | Optional | - | Appeal status (default: "Pending") |
| **City** | String | Optional | - | City name |
| **Street** | String | Optional | - | Street name |
| **BuildingNumber** | String | Optional | - | Building number/address |
| **District** | String | Optional | 150 chars | Administrative district |
| **Oblast** | String | Optional | 100 chars | Regional area (Oblast/Province) |
| **Postcode** | String | Optional | 20 chars | Postal/ZIP code |
| **Latitude** | Decimal | ✅ Required | - | Geographic latitude |
| **Longitude** | Decimal | ✅ Required | - | Geographic longitude |

---

## CSV Examples

### Example 1: Full Details with All Address Fields
```csv
UserId,CategoryId,Title,Description,Status,City,Street,BuildingNumber,District,Oblast,Postcode,Latitude,Longitude
550e8400-e29b-41d4-a716-446655440100,550e8400-e29b-41d4-a716-446655440000,Pothole,Large hole,Pending,Springfield,Main St,123,District 1,Western Oblast,12345,39.7817,-89.2500
550e8400-e29b-41d4-a716-446655440101,550e8400-e29b-41d4-a716-446655440001,Street Light,Not working,,Springfield,Oak Ave,45,District 2,Western Oblast,12346,39.7819,-89.2501
```

### Example 2: Minimal (only required fields + coordinates)
```csv
UserId,CategoryId,Title,Description,Status,City,Street,BuildingNumber,District,Oblast,Postcode,Latitude,Longitude
550e8400-e29b-41d4-a716-446655440100,550e8400-e29b-41d4-a716-446655440000,Pothole,Large hole,,,,,,,39.7817,-89.2500
```

### Example 3: Mixed Optional Fields
```csv
UserId,CategoryId,Title,Description,Status,City,Street,BuildingNumber,District,Oblast,Postcode,Latitude,Longitude
550e8400-e29b-41d4-a716-446655440100,550e8400-e29b-41d4-a716-446655440000,Pothole,Large hole,Pending,Springfield,Main St,123,,Western Oblast,12345,39.7817,-89.2500
```

---

## Address Data Storage

When an appeal is created from CSV, the address information is stored in the **Address** table with the following mapping:

| CSV Field | Database Column | Table |
|-----------|-----------------|-------|
| City | city | addresses |
| Street | street | addresses |
| BuildingNumber | building_number | addresses |
| District | district | addresses |
| Oblast | oblast | addresses |
| Postcode | postcode | addresses |
| Latitude | latitude | addresses |
| Longitude | longitude | addresses |

---

## Processing Logic

### Address Field Priority

1. **From CSV (Preferred)**
   - District, Oblast, Postcode read from CSV if provided
   - These values take priority over geocoding results

2. **From Geolocation (Fallback)**
   - If City or Street missing, coordinates are geocoded
   - Geocode may fill in City, Street, District, Oblast
   - CSV-provided values override geocoding results

3. **Example Flow**
   ```
   CSV Input: City="Springfield", Street="Main St", District="District 1"
         ↓
   Coordinate Geocoding: City and Street already provided, skip geocoding
         ↓
   Stored: City="Springfield", Street="Main St", District="District 1"
   
   ---
   
   CSV Input: City="", Street="", District="District 1"
         ↓
   Coordinate Geocoding: Retrieve City, Street from coordinates
         ↓
   Stored: City=(from geocode), Street=(from geocode), District="District 1"
   ```

---

## Code Changes

### 1. AppealCsvRecord
**File:** `AppealController.cs`

Added fields:
```csharp
public string? District { get; set; }
public string? Oblast { get; set; }
public string? Postcode { get; set; }
```

### 2. CreateProblemDto
**File:** `ProblemDto.cs`

Added fields:
```csharp
public string? District { get; set; }
public string? Oblast { get; set; }
public string? Postcode { get; set; }
```

### 3. ProblemService.CreateProblem()
**File:** `ProblemService.cs`

Logic to assign address fields:
```csharp
// Assign optional address fields from DTO
if (problem.Address != null)
{
    if (!string.IsNullOrEmpty(createProblemDto.District))
        problem.Address.District = createProblemDto.District;
    if (!string.IsNullOrEmpty(createProblemDto.Oblast))
        problem.Address.Oblast = createProblemDto.Oblast;
    if (!string.IsNullOrEmpty(createProblemDto.Postcode))
        problem.Address.Postcode = createProblemDto.Postcode;
}
```

---

## Backward Compatibility

✅ **Fully Backward Compatible**

- CSV files without these columns still work
- Fields are entirely optional
- Old CSV format:
  ```csv
  UserId,CategoryId,Title,Description,Status,City,Street,BuildingNumber,Latitude,Longitude
  ```
  Still works with new system

---

## Sample CSV Updated

The `sample_appeals.csv` has been updated with example data showing all new fields:

```csv
UserId,CategoryId,Title,Description,Status,City,Street,BuildingNumber,District,Oblast,Postcode,Latitude,Longitude
550e8400-e29b-41d4-a716-446655440100,550e8400-e29b-41d4-a716-446655440000,Pothole on Main Street,Large pothole...,Pending,Springfield,Main St,123,District 1,Western Oblast,12345,39.7817,-89.2500
```

---

## Testing

### Test 1: Upload CSV with New Fields
```bash
curl -X POST "http://localhost:5000/api/appeal/upload-csv" \
  -H "Authorization: Bearer [JWT_TOKEN]" \
  -F "CsvFile=@sample_appeals.csv"
```

Expected: All appeals created with District, Oblast, Postcode stored

### Test 2: Query Address Data
```bash
-- Check PostgreSQL
SELECT city, street, district, oblast, postcode
FROM addresses
WHERE district IS NOT NULL
LIMIT 5;
```

Expected: District and Oblast populated for new records

### Test 3: Backward Compatibility
Use old CSV format (without District, Oblast, Postcode):
```csv
UserId,CategoryId,Title,Description,Status,City,Street,BuildingNumber,Latitude,Longitude
550e8400...,550e8400...,Test,Description,Pending,City,Street,123,39.7817,-89.2500
```

Expected: Appeal created successfully (District/Oblast/Postcode NULL)

---

## Database Impact

No migration needed! The Address table already has these columns:
```sql
ALTER TABLE addresses ADD COLUMN district VARCHAR(150);
ALTER TABLE addresses ADD COLUMN oblast VARCHAR(100);
ALTER TABLE addresses ADD COLUMN postcode VARCHAR(20);
```

These already exist in the schema.

---

## Files Modified

1. **AppealCsvMap.cs**
   - Added District, Oblast, Postcode properties
   - Added CSV mappings for new fields

2. **ProblemDto.cs**
   - Added fields to CreateProblemDto

3. **ProblemService.cs**
   - Updated CreateProblem() to assign these fields to Address

4. **AppealController.cs**
   - Updated CSV upload logic to pass these fields

5. **sample_appeals.csv**
   - Updated with example data showing all fields

---

## Next Steps (Optional)

- Update frontend forms to include District, Oblast, Postcode fields
- Add validation for postcode format per region
- Display these fields in address details views
- Create district/oblast dropdown menus (lookup tables)

---

## Summary

✅ District, Oblast, Postcode now supported in CSV imports  
✅ Fields are stored in Address table  
✅ Fully backward compatible  
✅ Optional fields (defaults to NULL)  
✅ No database migration needed  
✅ Ready for production deployment  


