# Admin CRUD Endpoints Documentation

This document describes all the complete CRUD endpoints for admin to manage city councils and users in the Governmental Reporting System.

## Base URL
```
https://api.yourserver.com/api/admin
```

## Authentication
All endpoints require:
- **Authorization**: Bearer token with `Admin` role
- **Content-Type**: `application/json`

---

## User Management Endpoints

### 1. Get All Users
**Endpoint:** `GET /users`

Retrieves all users in the system with their role and status information.

**Response:**
```json
{
  "success": true,
  "message": "Successfully retrieved 5 user(s)",
  "totalCount": 5,
  "users": [
    {
      "userId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
      "fullName": "John Doe",
      "email": "john@example.com",
      "role": "User",
      "isActive": true,
      "addressId": "550e8400-e29b-41d4-a716-446655440000"
    }
  ]
}
```

**Status Codes:**
- `200`: Users retrieved successfully
- `401`: Unauthorized
- `403`: Forbidden - user does not have admin role
- `500`: Server error occurred

---

### 2. Get User by ID
**Endpoint:** `GET /users/{userId}`

Retrieves detailed information about a specific user.

**Parameters:**
- `userId` (path, required): The ID of the user to retrieve

**Response:**
```json
{
  "userId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "fullName": "John Doe",
  "email": "john@example.com",
  "googleAuthId": null,
  "addressId": "550e8400-e29b-41d4-a716-446655440000",
  "isActive": true,
  "address": {
    "addressId": "550e8400-e29b-41d4-a716-446655440000",
    "city": "Kyiv",
    "street": "Main Street",
    "buildingNumber": "123",
    "district": "Pechersk",
    "oblast": "Kyiv City",
    "country": "Ukraine",
    "postcode": "03150",
    "latitude": 50.4501,
    "longitude": 30.5234
  },
  "admin": null,
  "councilEmployee": null
}
```

**Status Codes:**
- `200`: User retrieved successfully
- `400`: Invalid user ID
- `401`: Unauthorized
- `403`: Forbidden
- `404`: User not found
- `500`: Server error occurred

---

### 3. Create User
**Endpoint:** `POST /users`

Creates a new user in the system.

**Request Body:**
```json
{
  "fullName": "Jane Smith",
  "email": "jane@example.com",
  "password": "SecurePassword123!",
  "googleAuthId": null,
  "city": "Lviv",
  "addressId": null
}
```

**Response:**
```json
{
  "success": true,
  "message": "User created successfully",
  "userId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "fullName": "Jane Smith",
  "email": "jane@example.com"
}
```

**Status Codes:**
- `201`: User created successfully
- `400`: Invalid user data (duplicate email, missing required fields)
- `401`: Unauthorized
- `403`: Forbidden
- `500`: Server error occurred

**Validation Rules:**
- Full name is required (not empty)
- Email is required (not empty, must be unique)
- Password is optional (if provided, will be hashed)

---

### 4. Update User
**Endpoint:** `PUT /users/{userId}`

Updates an existing user in the system.

**Parameters:**
- `userId` (path, required): The ID of the user to update

**Request Body:**
```json
{
  "userId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "fullName": "Jane Smith Updated",
  "email": "jane.updated@example.com",
  "password": "NewSecurePassword456!",
  "googleAuthId": null,
  "city": "Lviv",
  "addressId": null,
  "isActive": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "User updated successfully",
  "user": {
    "userId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
    "fullName": "Jane Smith Updated",
    "email": "jane.updated@example.com",
    "role": "User",
    "isActive": true,
    "addressId": null
  }
}
```

**Status Codes:**
- `200`: User updated successfully
- `400`: Invalid user data or ID mismatch
- `401`: Unauthorized
- `403`: Forbidden
- `404`: User not found
- `500`: Server error occurred

**Important:**
- The `userId` in the request body must match the `userId` in the URL path
- Email cannot be changed to an email already in use by another user

---

### 5. Delete User
**Endpoint:** `DELETE /users/{userId}`

Deletes a user from the system.

**Parameters:**
- `userId` (path, required): The ID of the user to delete

**Response:**
```json
{
  "success": true,
  "message": "User deleted successfully"
}
```

**Status Codes:**
- `200`: User deleted successfully
- `400`: Invalid user ID
- `401`: Unauthorized
- `403`: Forbidden
- `404`: User not found
- `500`: Server error occurred

---

## City Council Management Endpoints

### 6. Get All City Councils
**Endpoint:** `GET /city-councils`

Retrieves all city councils in the system with their address information.

**Response:**
```json
[
  {
    "councilId": "a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6",
    "name": "Kyiv City Council",
    "contactEmail": "contact@kyiv.ua",
    "city": "Kyiv",
    "district": "Pechersk",
    "oblast": "Kyiv City"
  },
  {
    "councilId": "b2c3d4e5-f6g7-48h9-i0j1-k2l3m4n5o6p7",
    "name": "Lviv City Council",
    "contactEmail": "contact@lviv.ua",
    "city": "Lviv",
    "district": "Halytskyi",
    "oblast": "Lviv"
  }
]
```

**Status Codes:**
- `200`: City councils retrieved successfully
- `401`: Unauthorized
- `403`: Forbidden
- `500`: Server error occurred

---

### 7. Get City Council by ID
**Endpoint:** `GET /city-councils/{councilId}`

Retrieves detailed information about a specific city council including employees and polls.

**Parameters:**
- `councilId` (path, required): The ID of the city council

**Response:**
```json
{
  "councilId": "a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6",
  "addressId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Kyiv City Council",
  "contactEmail": "contact@kyiv.ua",
  "address": {
    "addressId": "550e8400-e29b-41d4-a716-446655440000",
    "city": "Kyiv",
    "street": "Grushevsky Street",
    "buildingNumber": "5",
    "district": "Pechersk",
    "oblast": "Kyiv City",
    "country": "Ukraine",
    "postcode": "01008",
    "latitude": 50.4534,
    "longitude": 30.5195
  },
  "councilEmployees": [
    {
      "employeeId": "c3d4e5f6-g7h8-49i0-j1k2-l3m4n5o6p7q8",
      "userId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
      "councilId": "a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6",
      "position": "Head"
    }
  ],
  "polls": []
}
```

**Status Codes:**
- `200`: City council retrieved successfully
- `400`: Invalid council ID
- `401`: Unauthorized
- `403`: Forbidden
- `404`: City council not found
- `500`: Server error occurred

---

### 8. Create City Council
**Endpoint:** `POST /city-councils`

Creates a new city council in the system.

**Request Body:**
```json
{
  "name": "Kharkiv City Council",
  "contactEmail": "contact@kharkiv.ua",
  "headUserId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "postcode": "61002",
  "latitude": 50.0038,
  "longitude": 36.2304
}
```

**Response:**
```json
{
  "success": true,
  "message": "City council created successfully",
  "councilId": "c3d4e5f6-g7h8-49i0-j1k2-l3m4n5o6p7q8",
  "councilName": "Kharkiv City Council"
}
```

**Status Codes:**
- `201`: City council created successfully
- `400`: Invalid city council data
- `401`: Unauthorized
- `403`: Forbidden
- `500`: Server error occurred

**Validation Rules:**
- Council name is required (not empty, must be unique)
- Head user ID is required (must exist in system)
- Coordinates (latitude, longitude) are used for geolocation

---

### 9. Update City Council
**Endpoint:** `PUT /city-councils/{councilId}`

Updates an existing city council.

**Parameters:**
- `councilId` (path, required): The ID of the city council to update

**Request Body:**
```json
{
  "councilId": "a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6",
  "addressId": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Kyiv City Council - Updated",
  "contactEmail": "new-contact@kyiv.ua"
}
```

**Response:**
```json
{
  "success": true,
  "message": "City council updated successfully",
  "councilId": "a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6",
  "councilName": "Kyiv City Council - Updated"
}
```

**Status Codes:**
- `200`: City council updated successfully
- `400`: Invalid city council data or ID mismatch
- `401`: Unauthorized
- `403`: Forbidden
- `404`: City council not found
- `500`: Server error occurred

**Important:**
- The `councilId` in the request body must match the `councilId` in the URL path
- Council name cannot be changed to a name already in use by another council

---

### 10. Delete City Council
**Endpoint:** `DELETE /city-councils/{councilId}`

Deletes a city council from the system. Also removes all associated council employees.

**Parameters:**
- `councilId` (path, required): The ID of the city council to delete

**Response:**
```json
{
  "success": true,
  "message": "City council deleted successfully"
}
```

**Status Codes:**
- `200`: City council deleted successfully
- `400`: Invalid council ID
- `401`: Unauthorized
- `403`: Forbidden
- `404`: City council not found
- `500`: Server error occurred

---

## CSV Import Endpoint

### 11. Load City Councils from CSV
**Endpoint:** `POST /city-councils/load-csv`

Loads city councils from a CSV file and imports them into the system.

**CSV Format:**
```csv
Name,ContactEmail
Kyiv City Council,contact@kyiv.ua
Lviv City Council,info@lviv.ua
Kharkiv City Council,info@kharkiv.ua
```

**Request:**
- Form data with file: `file` (multipart/form-data)

**Response:**
```json
{
  "success": true,
  "message": "Successfully imported 3 city council(s)",
  "totalCount": 3,
  "loadedCouncils": [
    {
      "councilId": "a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6",
      "name": "Kyiv City Council",
      "contactEmail": "contact@kyiv.ua",
      "addressId": "550e8400-e29b-41d4-a716-446655440000"
    }
  ]
}
```

**Status Codes:**
- `200`: City councils imported successfully
- `400`: Invalid CSV file format or content
- `401`: Unauthorized
- `403`: Forbidden
- `500`: Server error occurred

---

## Common Error Responses

### Invalid Request
```json
{
  "success": false,
  "message": "User data cannot be null"
}
```

### Not Found
```json
{
  "success": false,
  "message": "User with ID 12345 not found"
}
```

### Conflict (Duplicate Email)
```json
{
  "success": false,
  "message": "User with email test@example.com already exists"
}
```

---

## cURL Examples

### Get All Users
```bash
curl -X GET https://api.yourserver.com/api/admin/users \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json"
```

### Create User
```bash
curl -X POST https://api.yourserver.com/api/admin/users \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Jane Smith",
    "email": "jane@example.com",
    "password": "SecurePassword123!"
  }'
```

### Update User
```bash
curl -X PUT https://api.yourserver.com/api/admin/users/d290f1ee-6c54-4b01-90e6-d701748f0851 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
    "fullName": "Jane Smith Updated",
    "email": "jane.updated@example.com",
    "isActive": true
  }'
```

### Delete User
```bash
curl -X DELETE https://api.yourserver.com/api/admin/users/d290f1ee-6c54-4b01-90e6-d701748f0851 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Create City Council
```bash
curl -X POST https://api.yourserver.com/api/admin/city-councils \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Kharkiv City Council",
    "contactEmail": "contact@kharkiv.ua",
    "headUserId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
    "latitude": 50.0038,
    "longitude": 36.2304
  }'
```

### Update City Council
```bash
curl -X PUT https://api.yourserver.com/api/admin/city-councils/a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "councilId": "a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6",
    "name": "Kharkiv City Council - Updated",
    "contactEmail": "new-contact@kharkiv.ua"
  }'
```

### Delete City Council
```bash
curl -X DELETE https://api.yourserver.com/api/admin/city-councils/a1b2c3d4-e5f6-47g8-h9i0-j1k2l3m4n5o6 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## Implementation Summary

All CRUD operations have been fully implemented:

### User Management (5 endpoints):
✅ GET /users - List all users
✅ GET /users/{userId} - Get specific user
✅ POST /users - Create new user
✅ PUT /users/{userId} - Update user
✅ DELETE /users/{userId} - Delete user

### City Council Management (6 endpoints):
✅ GET /city-councils - List all councils
✅ GET /city-councils/{councilId} - Get specific council
✅ POST /city-councils - Create new council
✅ PUT /city-councils/{councilId} - Update council
✅ DELETE /city-councils/{councilId} - Delete council
✅ POST /city-councils/load-csv - Import from CSV

### Service Layer:
✅ IAdminService - Updated with CRUD methods
✅ AdminService - Complete implementation
✅ ICityCouncilService - Updated with update/delete methods
✅ CityCouncilService - Complete implementation

### API Layer:
✅ AdminController - All endpoints implemented
✅ Response DTOs - Added for all operations

### Features:
- Comprehensive input validation
- Proper error handling with meaningful messages
- Logging for all operations
- Password hashing for user passwords
- Duplicate email prevention
- Duplicate council name prevention
- Cascading delete for council employees when council is deleted
- Full support for URL-based and request-body-based parameters

