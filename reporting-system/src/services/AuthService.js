const API_BASE_URL = 'http://localhost:5216';

class AuthService {
  // Register new user
  static async register(fullName, email, password, confirmPassword, city, district, oblast) {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Auth/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          FullName: fullName,
          Email: email,
          Password: password,
          ConfirmPassword: confirmPassword,
          City: city,           // <-- Add this line
          District: district,
          Oblast: oblast
        }),
      });

      const data = await response.json();

      if (data.success) {
        // Store tokens in localStorage
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('tokenType', data.tokenType);
        localStorage.setItem('expiresIn', data.expiresIn);

        // Prepare user object
        let user = data.user;

        // Check if address data is in separate object (backend returning address separately)
        if (data.address) {
          console.log('Address data received during registration:', data.address);

          // Add district and oblast to user object
          if (data.address.district) {
            user.district = data.address.district;
            localStorage.setItem('userDistrict', data.address.district);
            console.log('Stored district from address:', data.address.district);
          }
          if (data.address.oblast) {
            user.oblast = data.address.oblast;
            localStorage.setItem('userOblast', data.address.oblast);
            console.log('Stored oblast from address:', data.address.oblast);
          }
        }

        // Store district and oblast if provided in params
        if (district) {
          localStorage.setItem('userDistrict', district);
          user.district = district;
        }
        if (oblast) {
          localStorage.setItem('userOblast', oblast);
          user.oblast = oblast;
        }

        localStorage.setItem('user', JSON.stringify(user));

        return {
          success: true,
          user: user,
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
      console.log('Login response:', data);

      if (data.success) {
        // Store tokens in localStorage
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('refreshToken', data.refreshToken);
        localStorage.setItem('tokenType', data.tokenType);
        localStorage.setItem('expiresIn', data.expiresIn);

        // Prepare user object and extract district/oblast
        let user = data.user;

        // Check if address data is in separate object (backend returning address separately)
        if (data.address) {
          console.log('Address data received:', data.address);

          // Add district and oblast to user object
          if (data.address.district) {
            user.district = data.address.district;
            localStorage.setItem('userDistrict', data.address.district);
            console.log('✅ Stored district from address:', data.address.district);
          }
          if (data.address.oblast) {
            user.oblast = data.address.oblast;
            localStorage.setItem('userOblast', data.address.oblast);
            console.log('✅ Stored oblast from address:', data.address.oblast);
          }
        }

        // Also check if district/oblast are directly in user object
        if (user && user.district) {
          localStorage.setItem('userDistrict', user.district);
          console.log('✅ Stored district from user object:', user.district);
        }
        if (user && user.oblast) {
          localStorage.setItem('userOblast', user.oblast);
          console.log('✅ Stored oblast from user object:', user.oblast);
        }

        localStorage.setItem('user', JSON.stringify(user));
        console.log('✅ User stored with district:', user.district);

        return {
          success: true,
          user: user,
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
    localStorage.removeItem('userDistrict');
    localStorage.removeItem('userOblast');
  }

  // Update user address
  static async updateUserAddress(district, oblast) {
    try {
      const token = this.getAccessToken();
      if (!token) {
        throw new Error('No access token available');
      }

      const response = await fetch(`${API_BASE_URL}/api/Auth/update-address`, {
        method: 'PUT',
        headers: this.getAuthHeaders(),
        body: JSON.stringify({
          district,
          oblast,
        }),
      });

      const data = await response.json();

      if (data.success) {
        // Update stored user with new address info
        const user = this.getUser();
        if (user) {
          user.district = district;
          user.oblast = oblast;
          localStorage.setItem('user', JSON.stringify(user));
        }

        // Also store district and oblast separately for persistence
        localStorage.setItem('userDistrict', district);
        localStorage.setItem('userOblast', oblast);

        return {
          success: true,
          user: data.user || user,
          message: data.message,
        };
      } else {
        return {
          success: false,
          errors: data.errors || ['Address update failed'],
          message: data.message,
        };
      }
    } catch (error) {
      return {
        success: false,
        errors: [error.message],
        message: 'An error occurred while updating address',
      };
    }
  }

  // Get stored user
  static getUser() {
    const userStr = localStorage.getItem('user');
    const user = userStr ? JSON.parse(userStr) : null;

    // If user doesn't have district but has saved it, restore it
    if (user && !user.district) {
      const savedDistrict = localStorage.getItem('userDistrict');
      const savedOblast = localStorage.getItem('userOblast');
      if (savedDistrict) {
        user.district = savedDistrict;
      }
      if (savedOblast) {
        user.oblast = savedOblast;
      }
    }

    return user;
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

