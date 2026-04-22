const API_BASE_URL = 'http://localhost:5216';

class AddressService {
  // Get list of oblasts (regions)
  static async getOblasts() {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Address/oblasts`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        console.error(`API Error: ${response.status} ${response.statusText}`);
        return {
          success: false,
          errors: [`API Error: ${response.status} ${response.statusText}`],
          oblasts: [],
        };
      }

      const data = await response.json();
      console.log('Oblasts response:', data);

      // Handle plain array response
      if (Array.isArray(data)) {
        return {
          success: true,
          oblasts: data,
        };
      }

      // Handle object response with success flag
      if (data.success) {
        return {
          success: true,
          oblasts: data.oblasts || data.data || [],
        };
      } else {
        return {
          success: false,
          errors: data.errors || ['Failed to fetch oblasts'],
          oblasts: [],
        };
      }
    } catch (error) {
      console.error('Error fetching oblasts:', error);
      return {
        success: false,
        errors: [error.message],
        oblasts: [],
      };
    }
  }

  // Get list of districts for a specific oblast
  static async getDistricts(oblast) {
    try {
      const response = await fetch(
        `${API_BASE_URL}/api/Address/districts/${(oblast)}`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      if (!response.ok) {
        console.error(`API Error: ${response.status} ${response.statusText}`);
        return {
          success: false,
          errors: [`API Error: ${response.status} ${response.statusText}`],
          districts: [],
        };
      }

      const data = await response.json();
      console.log('Districts response:', data);

      // Handle plain array response
      if (Array.isArray(data)) {
        return {
          success: true,
          districts: data,
        };
      }

      // Handle object response with success flag
      if (data.success) {
        return {
          success: true,
          districts: data.districts || data.data || [],
        };
      } else {
        return {
          success: false,
          errors: data.errors || ['Failed to fetch districts'],
          districts: [],
        };
      }
    } catch (error) {
      console.error('Error fetching districts:', error);
      return {
        success: false,
        errors: [error.message],
        districts: [],
      };
    }
  }

  // Get both oblasts and districts (can be used for initial data load)
  static async getAddressOptions() {
    try {
      const oblastResult = await this.getOblasts();
      return {
        success: oblastResult.success,
        oblasts: oblastResult.oblasts,
      };
    } catch (error) {
      return {
        success: false,
        errors: [error.message],
        oblasts: [],
      };
    }
  }
}

export default AddressService;

