import { jwtDecode } from 'jwt-decode';

const API_BASE_URL = 'http://localhost:5216';

class AuthService {
  // Helper method to extract roles from JWT
  static extractRolesFromToken(token) {
    try {
      const decodedToken = jwtDecode(token);
      // ASP.NET Core often uses standard schema URLs for claims, so we check both
      const extractedRole = decodedToken.role || decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      if (extractedRole) {
        // Normalize to an array (if user has 1 role, ASP.NET might send a string instead of an array)
        return Array.isArray(extractedRole) ? extractedRole : [extractedRole];
      }
    } catch (decodeError) {
      console.error('Failed to decode JWT:', decodeError);
    }
    return [];
  }

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
          City: city,
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
        let user = data.user || {};

        // --- NEW: Decode JWT to get roles on Register ---
        const userRoles = this.extractRolesFromToken(data.accessToken);
        user.roles = userRoles;
        user.role = userRoles[0] || null;
        user.isAdmin = userRoles.includes('Admin');
        user.councilId = data.councilId ?? user.councilId ?? null;
        user.councilPosition = data.councilPosition ?? user.councilPosition ?? null;
        user.isCouncilHead = Boolean(data.isCouncilHead);
        console.log('🔐 Roles extracted on register:', userRoles);

        // Check if address data is in separate object
        if (data.address) {
          if (data.address.district) {
            user.district = data.address.district;
            localStorage.setItem('userDistrict', data.address.district);
          }
          if (data.address.oblast) {
            user.oblast = data.address.oblast;
            localStorage.setItem('userOblast', data.address.oblast);
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

        localStorage.setItem('councilId', user.councilId ?? '');

        localStorage.setItem('user', JSON.stringify(user));

        return {
          success: true,
          user: user,
          accessToken: data.accessToken,
          message: data.message,
        };
      } else {
        // Parse ASP.NET Core validation errors
        let parsedErrors = [];
        if (data.errors && typeof data.errors === 'object' && !Array.isArray(data.errors)) {
          Object.values(data.errors).forEach(errArr => {
            if (Array.isArray(errArr)) parsedErrors.push(...errArr);
          });
        } else if (Array.isArray(data.errors)) {
          parsedErrors = data.errors;
        }

        return {
          success: false,
          errors: parsedErrors.length > 0 ? parsedErrors : [data.message || 'Registration failed'],
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

        // Prepare user object
        let user = data.user || {};

        // --- NEW: Decode JWT to get roles on Login ---
        const userRoles = this.extractRolesFromToken(data.accessToken);
        user.roles = userRoles;
        user.role = userRoles[0] || null;
        user.isAdmin = userRoles.includes('Admin');
        user.councilId = data.councilId ?? user.councilId ?? null;
        user.councilPosition = data.councilPosition ?? user.councilPosition ?? null;
        user.isCouncilHead = Boolean(data.isCouncilHead);
        console.log('🔐 Roles extracted on login:', userRoles);

        // Check if address data is in separate object
        if (data.address) {
          if (data.address.district) {
            user.district = data.address.district;
            localStorage.setItem('userDistrict', data.address.district);
          }
          if (data.address.oblast) {
            user.oblast = data.address.oblast;
            localStorage.setItem('userOblast', data.address.oblast);
          }
        }

        // Also check if district/oblast are directly in user object
        if (user && user.district) localStorage.setItem('userDistrict', user.district);
        if (user && user.oblast) localStorage.setItem('userOblast', user.oblast);
        localStorage.setItem('councilId', user.councilId ?? '');

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
    localStorage.removeItem('councilId');
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

          if (user.councilId) {
            localStorage.setItem('councilId', user.councilId);
          }
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

    if (user && (user.councilId === undefined || user.councilId === null)) {
      const savedCouncilId = localStorage.getItem('councilId');
      if (savedCouncilId) {
        user.councilId = savedCouncilId;
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

        // Re-extract roles on token refresh just in case they were updated
        const user = this.getUser();
        if (user) {
          const userRoles = this.extractRolesFromToken(data.accessToken);
          user.roles = userRoles;
          user.role = userRoles[0] || null;
          user.isAdmin = userRoles.includes('Admin');
          user.councilId = data.councilId ?? user.councilId ?? null;
          user.councilPosition = data.councilPosition ?? user.councilPosition ?? null;
          user.isCouncilHead = Boolean(data.isCouncilHead);
          localStorage.setItem('councilId', user.councilId ?? '');
          localStorage.setItem('user', JSON.stringify(user));
        }

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