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
    console.log('🔄 Transforming appeal:', appeal);
    const transformed = {
      appealId: appeal.AppealId || appeal.appealId,
      categoryIconUrl: appeal.CategoryIconUrl || appeal.categoryIconUrl,
      latitude: appeal.Latitude || appeal.latitude,
      longitude: appeal.Longitude || appeal.longitude,
      // Preserve any additional properties
      ...appeal
    };
    console.log('✅ Transformed appeal:', transformed);
    return transformed;
  }

  /**
   * Fetch appeals from multiple districts
   * @param {string[]} districts - Array of district names/IDs
   * @returns {Promise<{success: boolean, appeals: Array, errors: string[]}>}
   */
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
