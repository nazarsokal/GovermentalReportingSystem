const API_BASE_URL = 'http://localhost:5216';

class AuthService {
  // Register new user
  static async register(fullName, email, password, confirmPassword) {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Auth/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          fullName,
          email,
          password,
          confirmPassword,
        }),
      });

      const data = await response.json();

      if (data.success) {
        // Store tokens in localStorage
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('tokenType', data.tokenType);
        localStorage.setItem('expiresIn', data.expiresIn);
        localStorage.setItem('user', JSON.stringify(data.user));

        return {
          success: true,
          user: data.user,
          accessToken: data.accessToken,
          message: data.message,
        };
      } else {
        return {
          success: false,
          errors: data.errors || ['Registration failed'],
          message: data.message,
        };
      }
    } catch (error) {
      return {
        success: false,
        errors: [error.message],
        message: 'An error occurred during registration',
      };
    }
  }

  // Login user
  static async login(email, password, rememberMe = false) {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email,
          password,
          rememberMe,
        }),
      });

      const data = await response.json();

      if (data.success) {
        // Store tokens in localStorage
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('tokenType', data.tokenType);
        localStorage.setItem('expiresIn', data.expiresIn);
        localStorage.setItem('user', JSON.stringify(data.user));

        return {
          success: true,
          user: data.user,
          accessToken: data.accessToken,
          message: data.message,
        };
      } else {
        return {
          success: false,
          errors: data.errors || ['Login failed'],
          message: data.message,
        };
      }
    } catch (error) {
      return {
        success: false,
        errors: [error.message],
        message: 'An error occurred during login',
      };
    }
  }

  // Logout user
  static logout() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenType');
    localStorage.removeItem('expiresIn');
    localStorage.removeItem('user');
  }

  // Get stored user
  static getUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  // Get access token
  static getAccessToken() {
    return localStorage.getItem('accessToken');
  }

  // Check if user is logged in
  static isLoggedIn() {
    return !!localStorage.getItem('accessToken');
  }

  // Get auth headers for API calls
  static getAuthHeaders() {
    const token = this.getAccessToken();
    const tokenType = localStorage.getItem('tokenType') || 'Bearer';
    return {
      'Authorization': `${tokenType} ${token}`,
      'Content-Type': 'application/json',
    };
  }

  // Refresh access token
  static async refreshToken() {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) {
        throw new Error('No refresh token available');
      }

      const response = await fetch(`${API_BASE_URL}/api/Auth/refresh`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          refreshToken,
        }),
      });

      const data = await response.json();

      if (data.success) {
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('tokenType', data.tokenType);
        return data.accessToken;
      } else {
        // If refresh fails, logout user
        this.logout();
        throw new Error('Token refresh failed');
      }
    } catch (error) {
      this.logout();
      throw error;
    }
  }
}

export default AuthService;

