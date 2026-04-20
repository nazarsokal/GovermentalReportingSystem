# Authentication Quick Reference

## API Base URL
```
http://localhost:5216
```

## Endpoints

### Register
```
POST /api/Auth/register
```

### Login  
```
POST /api/Auth/login
```

### Refresh Token
```
POST /api/Auth/refresh
```

## Service Usage

### AuthService (Authentication)
Located: `src/services/AuthService.js`

```javascript
// Register
const result = await AuthService.register(fullName, email, password, confirmPassword);

// Login
const result = await AuthService.login(email, password, rememberMe);

// Logout
AuthService.logout();

// Get current user
const user = AuthService.getUser();

// Get access token
const token = AuthService.getAccessToken();

// Check if logged in
const isLoggedIn = AuthService.isLoggedIn();

// Get auth headers for API calls
const headers = AuthService.getAuthHeaders();

// Refresh token
const newToken = await AuthService.refreshToken();
```

### ApiService (API Calls)
Located: `src/services/ApiService.js`

```javascript
// GET request
const data = await ApiService.get('/api/endpoint');

// POST request
const data = await ApiService.post('/api/endpoint', { key: 'value' });

// PUT request  
const data = await ApiService.put('/api/endpoint', { key: 'value' });

// DELETE request
const data = await ApiService.delete('/api/endpoint');
```

All ApiService requests:
- Include authentication headers automatically
- Refresh tokens if 401 response received
- Throw errors if refresh fails

## Token Management

### Storage Keys
- `accessToken` - JWT access token
- `refreshToken` - JWT refresh token  
- `tokenType` - Token type (Bearer)
- `expiresIn` - Expiration time
- `user` - User object (JSON)

### Token Lifecycle
1. User registers/logs in
2. Server returns access and refresh tokens
3. Tokens stored in localStorage
4. Access token sent with every API request
5. When access token expires:
   - Server returns 401
   - ApiService uses refresh token to get new access token
   - Request is retried with new token
6. When refresh token expires:
   - Refresh fails
   - User is logged out
   - Redirected to login page

## Response Format

### Success Response
```json
{
  "success": true,
  "accessToken": "eyJ...",
  "refreshToken": "eyJ...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "userId": "uuid",
    "fullName": "Full Name",
    "email": "email@example.com",
    "googleAuthId": null,
    "isActive": true
  },
  "message": "Operation successful",
  "errors": []
}
```

### Error Response
```json
{
  "success": false,
  "accessToken": null,
  "refreshToken": null,
  "expiresIn": 0,
  "tokenType": null,
  "user": null,
  "message": "Error message",
  "errors": ["error 1", "error 2"]
}
```

## File Structure
```
src/
├── services/
│   ├── AuthService.js     # Authentication & token management
│   └── ApiService.js      # API calls with token refresh
├── context/
│   └── AuthContext.js     # React context for auth state
├── pages/
│   ├── LoginPage.js       # Login & registration
│   └── DashboardPage.js   # Main app
└── components/
    ├── Navbar.js
    ├── StatCard.js
    ├── CityMap.js
    └── RecentAppeals.js
```

## Common Tasks

### Login User
```javascript
const result = await AuthService.login('user@example.com', 'password');
if (result.success) {
  // User logged in, tokens stored
  console.log('Welcome', result.user.fullName);
}
```

### Make Protected API Call
```javascript
try {
  const data = await ApiService.get('/api/issues');
  // Token automatically refreshed if needed
} catch (error) {
  // Handle error (includes auto-logout on token failure)
}
```

### Get Current User
```javascript
const user = AuthService.getUser();
console.log(user.fullName, user.email);
```

### Logout
```javascript
AuthService.logout();
// Tokens cleared, user can login again
```

## Environment Variables
See `.env.example` for configuration options.

Default configuration in code:
- API Base URL: `http://localhost:5216`
- Token storage: Browser localStorage

