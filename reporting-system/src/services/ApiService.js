import AuthService from './AuthService';

const API_BASE_URL = 'http://localhost:5216';

class ApiService {
  static async makeRequest(url, options = {}) {
    const headers = {
      ...AuthService.getAuthHeaders(),
      ...options.headers,
    };

    let response = await fetch(`${API_BASE_URL}${url}`, {
      ...options,
      headers,
    });

    // If unauthorized, try to refresh token
    if (response.status === 401) {
      try {
        await AuthService.refreshToken();
        const newHeaders = AuthService.getAuthHeaders();
        response = await fetch(`${API_BASE_URL}${url}`, {
          ...options,
          headers: newHeaders,
        });
      } catch (error) {
        throw new Error('Session expired. Please login again.');
      }
    }

    return response;
  }

  // GET request
  static async get(url) {
    const response = await this.makeRequest(url, {
      method: 'GET',
    });

    if (!response.ok) {
      throw new Error(`API request failed: ${response.statusText}`);
    }

    return response.json();
  }

  // POST request
  static async post(url, data) {
    const response = await this.makeRequest(url, {
      method: 'POST',
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error(`API request failed: ${response.statusText}`);
    }

    return response.json();
  }

  // PUT request
  static async put(url, data) {
    const response = await this.makeRequest(url, {
      method: 'PUT',
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error(`API request failed: ${response.statusText}`);
    }

    return response.json();
  }

  // PATCH request
  static async patch(url, data) {
    const response = await this.makeRequest(url, {
      method: 'PATCH',
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error(`API request failed: ${response.statusText}`);
    }

    return response.json();
  }

  // DELETE request
  static async delete(url) {
    const response = await this.makeRequest(url, {
      method: 'DELETE',
    });

    if (!response.ok) {
      throw new Error(`API request failed: ${response.statusText}`);
    }

    return response.json();
  }
}

export default ApiService;

