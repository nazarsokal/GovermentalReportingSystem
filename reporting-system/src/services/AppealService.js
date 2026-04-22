import ApiService from './ApiService';

class AppealService {
  /**
   * Fetch all appeals with their locations and category icons
   * @returns {Promise<{success: boolean, appeals: Array, errors: string[]}>}
   */
  static async getAppealLocations() {
    try {
      const appeals = await ApiService.get('/api/appeals/locations');
      const transformedAppeals = (appeals || []).map(appeal => this.transformAppealToCamelCase(appeal));
      return {
        success: true,
        appeals: transformedAppeals,
        errors: []
      };
    } catch (error) {
      console.error('Failed to fetch appeal locations:', error);
      return {
        success: false,
        appeals: [],
        errors: [error.message || 'Failed to fetch appeal locations']
      };
    }
  }

  /**
   * Fetch appeals for a specific district
   * @param {string} districtId - The district ID or name
   * @returns {Promise<{success: boolean, appeals: Array, errors: string[]}>}
   */
  static async getAppealLocationsByDistrict(districtId) {
    try {
      const appeals = await ApiService.get(`/api/Appeal/map/${encodeURIComponent(districtId)}`);
      const transformedAppeals = (appeals || []).map(appeal => this.transformAppealToCamelCase(appeal));
      return {
        success: true,
        appeals: transformedAppeals,
        errors: []
      };
    } catch (error) {
      console.error(`Failed to fetch appeal locations for district ${districtId}:`, error);
      return {
        success: false,
        appeals: [],
        errors: [error.message || `Failed to fetch appeal locations for district ${districtId}`]
      };
    }
  }

  /**
   * Transform PascalCase response to camelCase
   * @param {Object} appeal - The appeal object from backend
   * @returns {Object} - The appeal object with camelCase properties
   */
  static transformAppealToCamelCase(appeal) {
    const transformed = {
      appealId: appeal.AppealId || appeal.appealId,
      categoryIconUrl: appeal.CategoryIconUrl || appeal.categoryIconUrl,
      latitude: appeal.Latitude || appeal.latitude,
      longitude: appeal.Longitude || appeal.longitude,
      // Preserve any additional properties
      ...appeal
    };
    return transformed;
  }

  /**
   * Fetch appeal summary by type and name
   * @param {string} type - Type of summary: 'city', 'oblast', or 'district'
   * @param {string} name - Name of the city, oblast, or district
   * @returns {Promise<{success: boolean, summary: Object, errors: string[]}>}
   */
  static async getAppealSummaryByType(type, name) {
    try {
      console.log(`📊 Fetching appeal summary for ${type}:`, name);
      const response = await ApiService.get(`/api/Appeal/summary/${encodeURIComponent(type)}/${encodeURIComponent(name)}`);
      console.log('✅ Appeal summary received:', response);

      // Check if response is an array (direct appeals list) or an object with stats
      let appeals = [];
      let stats = {
        total: 0,
        pending: 0,
        inProgress: 0,
        resolved: 0
      };

      if (Array.isArray(response)) {
        // Backend returns appeals array directly
        appeals = response;

        // Calculate stats from appeals
        stats.total = appeals.length;
        stats.pending = appeals.filter(a => (a.Status || a.status || '').toUpperCase() === 'PENDING').length;
        stats.inProgress = appeals.filter(a => (a.Status || a.status || '').toUpperCase() === 'INPROGRESS' || (a.Status || a.status || '').toUpperCase() === 'IN PROGRESS').length;
        stats.resolved = appeals.filter(a => (a.Status || a.status || '').toUpperCase() === 'RESOLVED').length;
      } else {
        // Backend returns an object with stats and appeals
        stats = {
          total: response.Total || response.total || 0,
          pending: response.Pending || response.pending || 0,
          inProgress: response.InProgress || response.inProgress || 0,
          resolved: response.Resolved || response.resolved || 0
        };
        appeals = response.Appeals || response.appeals || [];
      }

      const transformed = {
        total: stats.total,
        pending: stats.pending,
        inProgress: stats.inProgress,
        resolved: stats.resolved,
        appeals: appeals
      };

      return {
        success: true,
        summary: transformed,
        errors: []
      };
    } catch (error) {
      console.error(`❌ Failed to fetch appeal summary for ${type} "${name}":`, error);
      return {
        success: false,
        summary: {
          total: 0,
          pending: 0,
          inProgress: 0,
          resolved: 0,
          appeals: []
        },
        errors: [error.message || `Failed to fetch appeal summary for ${type} "${name}"`]
      };
    }
  }

  // ...existing code...

  /**
   * Fetch recent appeals for the dashboard
   * @param {number} limit - Maximum number of appeals to return (default 5)
   * @returns {Promise<{success: boolean, appeals: Array, errors: string[]}>}
   */
  static async getRecentAppeals(limit = 5) {
    try {
      console.log('📋 Fetching recent appeals, limit:', limit);
      const appeals = await ApiService.get(`/api/Appeal/recent?limit=${limit}`);
      const transformedAppeals = (appeals || []).map(appeal => ({
        ...appeal,
        id: appeal.AppealId || appeal.appealId,
        title: appeal.ProblemName || appeal.problemName || 'Issue',
        issue: appeal.Description || appeal.description || '',
        status: appeal.Status || appeal.status || 'Pending',
        date: appeal.DatePublished ? new Date(appeal.DatePublished).toLocaleDateString('en-US', {
          year: 'numeric',
          month: 'short',
          day: 'numeric'
        }) : new Date().toLocaleDateString()
      }));
      console.log('✅ Recent appeals received:', transformedAppeals);
      return {
        success: true,
        appeals: transformedAppeals,
        errors: []
      };
    } catch (error) {
      console.error('❌ Failed to fetch recent appeals:', error);
      return {
        success: false,
        appeals: [],
        errors: [error.message || 'Failed to fetch recent appeals']
      };
    }
  }

  // ...existing code...
  static async getAppealLocationsFromMultipleDistricts(districts) {
    try {
      console.log('📍 getAppealLocationsFromMultipleDistricts called with districts:', districts);

      if (!districts || districts.length === 0) {
        console.warn('⚠️ No districts provided');
        return {
          success: true,
          appeals: [],
          errors: []
        };
      }

      // Fetch appeals from all districts in parallel
      console.log('🔄 Fetching appeals from', districts.length, 'districts');
      const promises = districts.map(district =>
        ApiService.get(`/api/Appeal/map/${encodeURIComponent(district)}`).catch(error => {
          console.error(`❌ Failed to fetch appeals for district ${district}:`, error);
          return [];
        })
      );

      const results = await Promise.all(promises);
      console.log('📦 Raw results from API:', results);

      // Flatten and deduplicate appeals by appealId
      const appealsMap = new Map();
      results.forEach((appeals, index) => {
        console.log(`📋 Processing result ${index} for district "${districts[index]}":`, appeals);
        if (Array.isArray(appeals)) {
          console.log(`   - Array with ${appeals.length} items`);
          appeals.forEach(appeal => {
            // Transform to camelCase
            const transformedAppeal = this.transformAppealToCamelCase(appeal);
            console.log(`   - Appeal ID: ${transformedAppeal.appealId}, Lat: ${transformedAppeal.latitude}, Lng: ${transformedAppeal.longitude}, Icon: ${transformedAppeal.categoryIconUrl}`);
            if (transformedAppeal.appealId) {
              appealsMap.set(transformedAppeal.appealId, transformedAppeal);
            } else {
              console.warn('   ⚠️ No appealId found in transformed appeal');
            }
          });
        } else {
          console.error(`   ❌ Result is not an array:`, typeof appeals, appeals);
        }
      });

      const finalAppeals = Array.from(appealsMap.values());
      console.log('✅ Final merged appeals:', finalAppeals);
      console.log(`   Total: ${finalAppeals.length} unique appeals`);

      return {
        success: true,
        appeals: finalAppeals,
        errors: []
      };
    } catch (error) {
      console.error('❌ Failed to fetch appeals from multiple districts:', error);
      return {
        success: false,
        appeals: [],
        errors: [error.message || 'Failed to fetch appeals from multiple districts']
      };
    }
  }
}

export default AppealService;
