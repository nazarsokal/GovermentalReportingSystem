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

  // Get coordinates for a specific district
  static async getDistrictCoordinates(district) {
    try {
      const response = await fetch(
        `${API_BASE_URL}/api/Address/districts/${encodeURIComponent(district)}/coordinates`,
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
          coordinates: null,
        };
      }

      const data = await response.json();
      console.log('District coordinates response:', data);

      // Handle direct coordinates object response
      if (data && data.latitude !== undefined && data.longitude !== undefined) {
        return {
          success: true,
          coordinates: {
            lat: data.latitude,
            lng: data.longitude,
          },
        };
      }

      // Handle object response with success flag
      if (data.success && data.data) {
        return {
          success: true,
          coordinates: {
            lat: data.data.latitude || data.data.lat,
            lng: data.data.longitude || data.data.lng,
          },
        };
      } else {
        return {
          success: false,
          errors: data.errors || ['Failed to fetch coordinates'],
          coordinates: null,
        };
      }
    } catch (error) {
      console.error('Error fetching district coordinates:', error);
      return {
        success: false,
        errors: [error.message],
        coordinates: null,
      };
    }
  }

  // Get all district coordinates at once (for caching)
  static async getAllDistrictCoordinates() {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Address/districts/coordinates/all`, {
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
          coordinates: {},
        };
      }

      const data = await response.json();
      console.log('All district coordinates response:', data);

      // Handle array response and convert to object
      if (Array.isArray(data)) {
        const coordinatesMap = {};
        data.forEach((item) => {
          if (item.district && item.latitude !== undefined && item.longitude !== undefined) {
            coordinatesMap[item.district] = {
              lat: item.latitude,
              lng: item.longitude,
            };
          }
        });
        return {
          success: true,
          coordinates: coordinatesMap,
        };
      }

      // Handle object response
      if (data.success && data.data) {
        return {
          success: true,
          coordinates: data.data,
        };
      } else {
        return {
          success: false,
          errors: data.errors || ['Failed to fetch coordinates'],
          coordinates: {},
        };
      }
    } catch (error) {
      console.error('Error fetching all district coordinates:', error);
      return {
        success: false,
        errors: [error.message],
        coordinates: {},
      };
    }
  }
}

export default AddressService;

