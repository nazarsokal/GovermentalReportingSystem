# API Integration & JWT Token Management Guide

## Overview

This document describes the JWT token-based authentication system implemented in the Urban Infrastructure Monitor application.

## JWT Token Management

### Token Storage

Tokens are stored in browser localStorage with the following keys:
- `accessToken` - JWT access token used for authenticated API requests
- `refreshToken` - JWT refresh token used to obtain new access tokens
- `tokenType` - Token type (typically "Bearer")
- `expiresIn` - Token expiration time in seconds
- `user` - JSON stringified user object

### AuthService Implementation

The `AuthService` class handles all authentication-related operations:

```javascript
import AuthService from './services/AuthService';

// Login
const result = await AuthService.login(email, password, rememberMe);

// Register
const result = await AuthService.register(fullName, email, password, confirmPassword);

// Logout
AuthService.logout();

// Get stored user
const user = AuthService.getUser();

// Get access token
const token = AuthService.getAccessToken();

// Get auth headers for API calls
const headers = AuthService.getAuthHeaders();
```

### API Service Integration

The `ApiService` class provides methods for making authenticated API calls with automatic token refresh:

```javascript
import ApiService from './services/ApiService';

// GET request
const data = await ApiService.get('/api/endpoint');

// POST request
const data = await ApiService.post('/api/endpoint', { key: 'value' });

// PUT request
const data = await ApiService.put('/api/endpoint', { key: 'value' });

// DELETE request
const data = await ApiService.delete('/api/endpoint');
```

### Token Refresh Flow

When an API call returns a 401 (Unauthorized) response:

1. The system attempts to refresh the access token using the refresh token
2. If refresh is successful, the original request is retried with the new token
3. If refresh fails, the user is logged out automatically
4. The user is redirected to the login page

```javascript
// Automatic refresh happens in ApiService.makeRequest()
try {
  response = await ApiService.get('/protected-endpoint');
} catch (error) {
  // If 401, refresh token is attempted automatically
  // User is logged out if refresh fails
}
```

## Authentication Endpoints

### POST /api/Auth/register

Register a new user account.

**Request:**
```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "password123",
  "confirmPassword": "password123"
}
```

**Response (Success):**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "fullName": "John Doe",
    "email": "john@example.com",
    "googleAuthId": null,
    "isActive": true
  },
  "message": "Registration successful",
  "errors": []
}
```

**Response (Error):**
```json
{
  "success": false,
  "accessToken": null,
  "refreshToken": null,
  "expiresIn": 0,
  "tokenType": null,
  "user": null,
  "message": "Registration failed",
  "errors": [
    "Email already exists",
    "Password is too weak"
  ]
}
```

### POST /api/Auth/login

Login with email and password.

**Request:**
```json
{
  "email": "john@example.com",
  "password": "password123",
  "rememberMe": true
}
```

**Response (Success):**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "fullName": "John Doe",
    "email": "john@example.com",
    "googleAuthId": null,
    "isActive": true
  },
  "message": "Login successful",
  "errors": []
}
```

### POST /api/Auth/refresh

Refresh the access token using a refresh token.

**Request:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Response (Success):**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "message": "Token refreshed successfully",
  "errors": []
}
```

## Using Auth in Components

### Option 1: Using AuthService Directly

```javascript
import AuthService from '../services/AuthService';

function MyComponent() {
  const handleLogin = async () => {
    const result = await AuthService.login('user@example.com', 'password');
    if (result.success) {
      // User logged in successfully
      const user = AuthService.getUser();
      console.log(user);
    }
  };

  return (
    <button onClick={handleLogin}>Login</button>
  );
}
```

### Option 2: Using AuthContext (Recommended)

```javascript
import { useAuth } from '../context/AuthContext';

function MyComponent() {
  const { user, isLoggedIn, login, logout } = useAuth();

  const handleLogin = async () => {
    const result = await login('user@example.com', 'password');
    if (result.success) {
      console.log('Logged in as:', user.fullName);
    }
  };

  if (!isLoggedIn) {
    return <button onClick={handleLogin}>Login</button>;
  }

  return (
    <div>
      <p>Welcome, {user.fullName}</p>
      <button onClick={logout}>Logout</button>
    </div>
  );
}
```

## Making Protected API Calls

### Using ApiService

```javascript
import ApiService from '../services/ApiService';

async function fetchInfrastructureIssues() {
  try {
    const data = await ApiService.get('/api/issues');
    console.log('Issues:', data);
  } catch (error) {
    console.error('Failed to fetch issues:', error.message);
  }
}

async function reportIssue(issueData) {
  try {
    const response = await ApiService.post('/api/issues', issueData);
    console.log('Issue reported:', response);
  } catch (error) {
    console.error('Failed to report issue:', error.message);
  }
}
```

### Request Headers

All API requests through `ApiService` automatically include the authorization header:

```
Authorization: Bearer <accessToken>
Content-Type: application/json
```

## Error Handling

### Authentication Errors

```javascript
const result = await AuthService.login(email, password);

if (result.success) {
  // Handle successful login
} else {
  // Handle errors
  const errorMessage = result.errors?.[0] || result.message;
  console.error('Login failed:', errorMessage);
}
```

### API Errors

```javascript
try {
  const data = await ApiService.get('/api/endpoint');
} catch (error) {
  if (error.message.includes('Session expired')) {
    // User was logged out due to token refresh failure
    // Redirect to login
  } else {
    console.error('API error:', error.message);
  }
}
```

## Token Expiration Handling

When a token expires:

1. The next API call will receive a 401 response
2. ApiService will automatically attempt to refresh the token
3. If refresh succeeds, the original request is retried
4. If refresh fails, the user is logged out

To implement token expiration warning:

```javascript
function TokenExpirationWarning() {
  const { accessToken } = useAuth();
  const [expiresIn, setExpiresIn] = useState(null);

  useEffect(() => {
    const stored = localStorage.getItem('expiresIn');
    if (stored) {
      const remaining = parseInt(stored) * 1000 - (Date.now() - Date.now());
      setExpiresIn(Math.floor(remaining / 1000));
    }

    const interval = setInterval(() => {
      const stored = localStorage.getItem('expiresIn');
      if (stored) {
        const remaining = parseInt(stored) * 1000 - (Date.now() - Date.now());
        setExpiresIn(Math.floor(remaining / 1000));
      }
    }, 1000);

    return () => clearInterval(interval);
  }, []);

  if (expiresIn && expiresIn < 300) {
    return <div className="warning">Your session expires in {expiresIn} seconds</div>;
  }

  return null;
}
```

## Security Best Practices

1. **Never store sensitive data in localStorage**
   - Only store tokens, not passwords
   - Clear tokens on logout

2. **Use HTTPS in production**
   - Always use HTTPS to prevent man-in-the-middle attacks
   - Never send tokens over unencrypted connections

3. **Token refresh strategy**
   - Use short-lived access tokens (5-15 minutes)
   - Use longer-lived refresh tokens (7-30 days)
   - Implement refresh token rotation

4. **CORS configuration**
   - Only allow requests from trusted domains
   - Set appropriate CORS headers on the backend

5. **Token validation**
   - Validate JWT signature on the backend
   - Check token expiration time
   - Verify token claims match user context

## Troubleshooting

### "Cannot resolve directory pages"
This is a normal IDE warning. The files are created correctly.

### Login fails with network error
Ensure the backend server is running on `http://localhost:5216`

### Tokens not persisting
Check that localStorage is enabled in your browser settings.

### Session expires without user interaction
This is normal if your token has a short expiration time. Implement automatic token refresh or token expiration warnings.

### CORS errors on API calls
Ensure your backend server has CORS enabled and allows requests from `localhost:3000`

