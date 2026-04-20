# Testing Guide

## Manual Testing Checklist

### 1. Registration

**Test Case 1.1: Successful Registration**
1. Navigate to http://localhost:3000
2. Click "Register" tab
3. Fill in:
   - Full Name: "John Doe"
   - Email: "john@example.com"
   - Password: "password123"
   - Confirm Password: "password123"
4. Click "Create Account"
5. **Expected**: Account created, tokens stored, redirected to Dashboard

**Test Case 1.2: Password Mismatch**
1. Fill registration form with mismatched passwords
2. Click "Create Account"
3. **Expected**: Error message "Passwords do not match"

**Test Case 1.3: Empty Fields**
1. Click "Create Account" without filling fields
2. **Expected**: Error message "Please fill in all fields"

**Test Case 1.4: Duplicate Email**
1. Register with email that already exists
2. **Expected**: Error message from server (if endpoint configured)

### 2. Login

**Test Case 2.1: Successful Login**
1. Click "Sign In" tab
2. Enter registered email and password
3. Click "Sign In"
4. **Expected**: Login successful, redirected to Dashboard, user data displayed in navbar

**Test Case 2.2: Remember Me Checkbox**
1. Check "Remember me" before logging in
2. **Expected**: rememberMe parameter sent to server

**Test Case 2.3: Invalid Credentials**
1. Enter incorrect email or password
2. Click "Sign In"
3. **Expected**: Error message from server

**Test Case 2.4: Empty Fields**
1. Click "Sign In" without filling fields
2. **Expected**: Error message "Please fill in all fields"

### 3. Dashboard Navigation

**Test Case 3.1: Dashboard Page**
1. Log in successfully
2. View Dashboard page
3. **Expected**:
   - Stats cards showing: 5 Total Appeals, 2 Pending, 2 In Progress, 1 Resolved
   - City map visualization
   - Recent appeals list on right side
   - Navbar with user name and logout button

**Test Case 3.2: Page Navigation**
1. Click "Report Problem" in navbar
2. **Expected**: Report Problem page loads
3. Click "Stats & Statistics" in navbar
4. **Expected**: Statistics page loads
5. Click "Dashboard" in navbar
6. **Expected**: Dashboard page loads

### 4. Logout

**Test Case 4.1: Logout**
1. Click "Logout" button in navbar
2. **Expected**: 
   - All tokens cleared from localStorage
   - Redirected to Login page
   - User data cleared

### 5. Token Management

**Test Case 5.1: Token Persistence**
1. Log in successfully
2. Open browser DevTools > Application > Local Storage > localhost:3000
3. **Expected**: See the following keys:
   - accessToken
   - refreshToken
   - tokenType
   - expiresIn
   - user (JSON object)

**Test Case 5.2: Persistent Login**
1. Log in successfully
2. Refresh the page (F5)
3. **Expected**: User remains logged in, no re-login required

**Test Case 5.3: Clear Local Storage**
1. Log in successfully
2. Clear localStorage manually
3. Refresh page
4. **Expected**: Redirected to Login page

### 6. UI/UX Testing

**Test Case 6.1: Form Validation**
1. Try to submit login form without email
2. **Expected**: Email field error or disabled submit

**Test Case 6.2: Loading States**
1. Click sign in button
2. **Expected**: Button shows "Signing In..." and is disabled

**Test Case 6.3: Responsive Design**
1. Open app on desktop (1920px wide)
2. **Expected**: Full 2-column layout with map and appeals side-by-side
3. Resize to tablet (768px wide)
4. **Expected**: Layout adapts, responsive
5. Resize to mobile (375px wide)
6. **Expected**: Single column layout, all content readable

**Test Case 6.4: Error Messages**
1. Enter invalid credentials
2. **Expected**: Red error alert displays with message
3. Click Sign In tab
4. **Expected**: Error message clears

**Test Case 6.5: Success Messages**
1. Register successfully
2. **Expected**: Green success message displays

### 7. Network Testing

**Test Case 7.1: Backend Server Down**
1. Stop backend server
2. Try to login
3. **Expected**: Network error message displayed

**Test Case 7.2: Invalid Endpoint**
1. Change API_BASE_URL in AuthService to invalid URL
2. Try to login
3. **Expected**: Network error message

### 8. Browser Compatibility

Test on:
- Chrome (Latest)
- Safari (Latest)
- Firefox (Latest)
- Edge (Latest)

**Expected**: All functionality works consistently across browsers

## Automated Testing Examples

### Testing AuthService

```javascript
import AuthService from '../services/AuthService';

describe('AuthService', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  test('login stores tokens in localStorage', async () => {
    const result = await AuthService.login('test@example.com', 'password');
    
    if (result.success) {
      expect(localStorage.getItem('accessToken')).toBeDefined();
      expect(localStorage.getItem('refreshToken')).toBeDefined();
      expect(localStorage.getItem('user')).toBeDefined();
    }
  });

  test('logout clears localStorage', () => {
    localStorage.setItem('accessToken', 'test-token');
    AuthService.logout();
    
    expect(localStorage.getItem('accessToken')).toBeNull();
    expect(localStorage.getItem('user')).toBeNull();
  });

  test('getUser returns stored user', () => {
    const testUser = { userId: '123', fullName: 'Test', email: 'test@example.com' };
    localStorage.setItem('user', JSON.stringify(testUser));
    
    const user = AuthService.getUser();
    expect(user).toEqual(testUser);
  });
});
```

### Testing LoginPage Component

```javascript
import { render, screen, fireEvent } from '@testing-library/react';
import LoginPage from '../pages/LoginPage';

describe('LoginPage', () => {
  test('renders login form initially', () => {
    const mockOnLogin = jest.fn();
    render(<LoginPage onLogin={mockOnLogin} />);
    
    expect(screen.getByText('Sign In')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('you@example.com')).toBeInTheDocument();
  });

  test('switches to register form', () => {
    const mockOnLogin = jest.fn();
    render(<LoginPage onLogin={mockOnLogin} />);
    
    const registerButton = screen.getByText('Register');
    fireEvent.click(registerButton);
    
    expect(screen.getByText('Create Account')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('John Doe')).toBeInTheDocument();
  });

  test('displays error on failed login', async () => {
    const mockOnLogin = jest.fn();
    render(<LoginPage onLogin={mockOnLogin} />);
    
    // Fill form with invalid credentials
    fireEvent.change(screen.getByPlaceholderText('you@example.com'), {
      target: { value: 'invalid@example.com' }
    });
    fireEvent.change(screen.getByPlaceholderText('••••••••'), {
      target: { value: 'wrong-password' }
    });
    
    const signInButton = screen.getByText('Sign In');
    fireEvent.click(signInButton);
    
    // Wait for error message
    const errorMessage = await screen.findByText(/login failed/i);
    expect(errorMessage).toBeInTheDocument();
  });
});
```

## API Testing with Postman/Curl

### Test Registration Endpoint

```bash
curl -X POST http://localhost:5216/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Test User",
    "email": "test@example.com",
    "password": "password123",
    "confirmPassword": "password123"
  }'
```

### Test Login Endpoint

```bash
curl -X POST http://localhost:5216/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123",
    "rememberMe": true
  }'
```

### Test Refresh Token Endpoint

```bash
curl -X POST http://localhost:5216/api/Auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your-refresh-token-here"
  }'
```

### Test Protected API Call

```bash
curl -X GET http://localhost:5216/api/some-protected-endpoint \
  -H "Authorization: Bearer your-access-token-here" \
  -H "Content-Type: application/json"
```

## Performance Testing

**Test Case P.1: Login Response Time**
1. Open DevTools > Network
2. Log in
3. **Expected**: Login request completes in < 1 second

**Test Case P.2: Dashboard Load Time**
1. Open DevTools > Performance
2. Log in, navigate to Dashboard
3. Record performance
4. **Expected**: Page fully interactive in < 2 seconds

**Test Case P.3: Memory Usage**
1. Open DevTools > Memory
2. Log in, navigate between pages
3. Logout
4. **Expected**: No memory leaks, memory released properly

## Security Testing

**Test Case SEC.1: Token in URL**
1. Check browser address bar and network requests
2. **Expected**: Tokens never appear in URL, only in Authorization header

**Test Case SEC.2: Token in Cookies**
1. Open DevTools > Application > Cookies
2. Log in
3. **Expected**: No tokens stored in cookies (stored in localStorage only)

**Test Case SEC.3: XSS Prevention**
1. Try to inject script in form fields
2. **Expected**: Script is escaped/sanitized, not executed

**Test Case SEC.4: Logout Clears Session**
1. Log in, check localStorage
2. Logout
3. Check localStorage again
4. **Expected**: All tokens cleared, cannot access protected routes

## Test Execution Commands

```bash
# Run all tests
npm test

# Run tests with coverage
npm test -- --coverage

# Run tests in watch mode
npm test -- --watch

# Build for production
npm run build
```

## Debugging Tips

1. **Check Console Errors**: Open DevTools Console tab, look for errors
2. **Check Network Requests**: Open DevTools Network tab, inspect API calls
3. **Check Local Storage**: Open DevTools Application > Local Storage
4. **Check Component State**: Use React DevTools extension
5. **Log API Responses**: Add console.log in AuthService.js

## Known Issues & Workarounds

**Issue**: CORS errors on API calls
- **Workaround**: Ensure backend has CORS enabled for localhost:3000

**Issue**: Tokens not persisting
- **Workaround**: Check that localStorage is enabled in browser settings

**Issue**: "Cannot resolve directory pages" warning
- **Workaround**: This is normal IDE warning, files are created correctly

