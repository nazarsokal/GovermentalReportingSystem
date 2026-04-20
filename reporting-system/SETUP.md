# Urban Infrastructure Monitor - Setup Guide

## Overview
This is a React-based web application for monitoring and reporting urban infrastructure problems. It includes user authentication with JWT tokens and a dashboard for tracking infrastructure issues.

## Features

- **User Authentication**
  - Login with email and password
  - User registration
  - JWT token-based authentication
  - Remember me functionality
  - Token refresh mechanism

- **Dashboard**
  - View infrastructure statistics
  - Interactive city map showing reported issues
  - Recent appeals list with status tracking
  - Multi-page navigation

- **Token Management**
  - Access token storage
  - Refresh token handling
  - Automatic token refresh on API calls
  - Persistent login using localStorage

## Prerequisites

- Node.js (v14 or higher)
- npm or yarn
- Backend server running at `http://localhost:5216`

## Installation

1. Install dependencies:
```bash
npm install
```

2. Start the development server:
```bash
npm start
```

The application will open at `http://localhost:3000`

## API Endpoints

### Authentication

#### Register
- **URL**: `POST /api/Auth/register`
- **Base URL**: `http://localhost:5216`
- **Request Body**:
```json
{
  "fullName": "string",
  "email": "string",
  "password": "string",
  "confirmPassword": "string"
}
```
- **Response**:
```json
{
  "success": true,
  "accessToken": "string",
  "refreshToken": "string",
  "expiresIn": 0,
  "tokenType": "string",
  "user": {
    "userId": "string (uuid)",
    "fullName": "string",
    "email": "string",
    "googleAuthId": "string",
    "isActive": true
  },
  "message": "string",
  "errors": ["string"]
}
```

#### Login
- **URL**: `POST /api/Auth/login`
- **Base URL**: `http://localhost:5216`
- **Request Body**:
```json
{
  "email": "string",
  "password": "string",
  "rememberMe": true
}
```
- **Response**: (Same as Register)

#### Refresh Token
- **URL**: `POST /api/Auth/refresh`
- **Request Body**:
```json
{
  "refreshToken": "string"
}
```

## Project Structure

```
src/
├── pages/
│   ├── LoginPage.js          # Login and registration page
│   └── DashboardPage.js      # Main dashboard with statistics
├── components/
│   ├── Navbar.js             # Navigation bar
│   ├── StatCard.js           # Statistics card component
│   ├── CityMap.js            # City infrastructure map
│   └── RecentAppeals.js      # Recent issues list
├── services/
│   ├── AuthService.js        # Authentication service with JWT handling
│   └── ApiService.js         # General API service with token management
├── styles/
│   ├── LoginPage.css
│   ├── DashboardPage.css
│   ├── Navbar.css
│   ├── StatCard.css
│   ├── CityMap.css
│   └── RecentAppeals.css
├── App.js                     # Main app component
├── App.css
├── index.js                   # React entry point
└── index.css
```

## Authentication Flow

1. **Registration**:
   - User fills in registration form (full name, email, password, confirm password)
   - Form data is sent to `/api/Auth/register` endpoint
   - Server returns user data and tokens
   - Tokens are stored in localStorage
   - User is redirected to dashboard

2. **Login**:
   - User enters email and password
   - Credentials sent to `/api/Auth/login` endpoint
   - Server validates and returns tokens
   - Tokens stored in localStorage
   - User is redirected to dashboard

3. **Token Management**:
   - Access token is used for authenticated API requests
   - Token is included in Authorization header: `Bearer <accessToken>`
   - If request fails with 401 (Unauthorized), refresh token is used to get new access token
   - If refresh fails, user is logged out

4. **Logout**:
   - User clicks logout button
   - Tokens are cleared from localStorage
   - User is redirected to login page

## Token Storage

Tokens and user data are stored in browser localStorage:
- `accessToken` - JWT access token
- `refreshToken` - JWT refresh token
- `tokenType` - Token type (usually "Bearer")
- `expiresIn` - Token expiration time
- `user` - JSON stringified user object

## Persistent Login

The application checks for stored tokens on mount. If valid tokens exist, the user is automatically logged in without needing to enter credentials again.

## Error Handling

- Invalid credentials show error messages
- Network errors are caught and displayed
- Token refresh failures trigger automatic logout
- Form validation prevents incomplete submissions

## Available Scripts

### Development
```bash
npm start
```
Runs the app in development mode. Open [http://localhost:3000](http://localhost:3000)

### Build
```bash
npm run build
```
Builds the app for production

### Testing
```bash
npm test
```
Runs the test suite

## Notes

- The application currently uses mock data for the dashboard
- The city map is a simplified SVG visualization
- Future enhancements can include real map integration and actual infrastructure data APIs

## Troubleshooting

### Server not running
If you see connection errors, ensure the backend server is running on `http://localhost:5216`

### Tokens not persisting
Check browser localStorage is enabled. In Chrome DevTools: Application > Storage > Local Storage

### CORS errors
The backend server must have CORS enabled to allow requests from localhost:3000

## Security Notes

- Never commit tokens to version control
- Use HTTPS in production
- Implement token expiration handling
- Consider using httpOnly cookies for token storage in production

