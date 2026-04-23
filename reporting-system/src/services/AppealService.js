import ApiService from './ApiService';

const API_BASE_URL = 'http://localhost:5216';

class AppealService {
  static async getAppealLocations() {
    try {
      const appeals = await ApiService.get('/api/appeals/locations');
      const transformedAppeals = (appeals || []).map(appeal => this.transformAppealToCamelCase(appeal));
      return { success: true, appeals: transformedAppeals, errors: [] };
    } catch (error) {
      console.error('Failed to fetch appeal locations:', error);
      return { success: false, appeals: [], errors: [error.message || 'Failed to fetch appeal locations'] };
    }
  }

  static async getAppealLocationsByDistrict(districtId) {
    try {
      const appeals = await ApiService.get(`/api/Appeal/map/${encodeURIComponent(districtId)}`);
      const transformedAppeals = (appeals || []).map(appeal => this.transformAppealToCamelCase(appeal));
      return { success: true, appeals: transformedAppeals, errors: [] };
    } catch (error) {
      console.error(`Failed to fetch appeal locations for district ${districtId}:`, error);
      return { success: false, appeals: [], errors: [error.message || `Failed to fetch appeal locations for district ${districtId}`] };
    }
  }

  static transformAppealToCamelCase(appeal) {
    return {
      appealId: appeal.AppealId || appeal.appealId,
      categoryIconUrl: appeal.CategoryIconUrl || appeal.categoryIconUrl,
      latitude: appeal.Latitude || appeal.latitude,
      longitude: appeal.Longitude || appeal.longitude,
      ...appeal
    };
  }

  static async getAppealSummaryByType(type, name) {
    try {
      const response = await ApiService.get(`/api/Appeal/summary/${encodeURIComponent(type)}/${encodeURIComponent(name)}`);

      let appeals = [];
      let stats = { total: 0, pending: 0, inProgress: 0, resolved: 0 };

      if (Array.isArray(response)) {
        appeals = response;
        stats.total = appeals.length;
        stats.pending = appeals.filter(a => (a.Status || a.status || '').toUpperCase() === 'PENDING').length;
        stats.inProgress = appeals.filter(a => (a.Status || a.status || '').toUpperCase() === 'INPROGRESS' || (a.Status || a.status || '').toUpperCase() === 'IN PROGRESS').length;
        stats.resolved = appeals.filter(a => (a.Status || a.status || '').toUpperCase() === 'RESOLVED').length;
      } else {
        stats = {
          total: response.Total || response.total || 0,
          pending: response.Pending || response.pending || 0,
          inProgress: response.InProgress || response.inProgress || 0,
          resolved: response.Resolved || response.resolved || 0
        };
        appeals = response.Appeals || response.appeals || [];
      }

      return {
        success: true,
        summary: { total: stats.total, pending: stats.pending, inProgress: stats.inProgress, resolved: stats.resolved, appeals: appeals },
        errors: []
      };
    } catch (error) {
      console.error(`Failed to fetch appeal summary for ${type} "${name}":`, error);
      return { success: false, summary: { total: 0, pending: 0, inProgress: 0, resolved: 0, appeals: [] }, errors: [error.message || 'Failed to fetch appeal summary'] };
    }
  }

  static async getRecentAppeals(limit = 5) {
    try {
      const appeals = await ApiService.get(`/api/Appeal/recent?limit=${limit}`);
      const transformedAppeals = (appeals || []).map(appeal => ({
        ...appeal,
        id: appeal.AppealId || appeal.appealId,
        title: appeal.ProblemName || appeal.problemName || 'Issue',
        issue: appeal.Description || appeal.description || '',
        status: appeal.Status || appeal.status || 'Pending',
        date: appeal.DatePublished ? new Date(appeal.DatePublished).toLocaleDateString() : new Date().toLocaleDateString()
      }));
      return { success: true, appeals: transformedAppeals, errors: [] };
    } catch (error) {
      return { success: false, appeals: [], errors: [error.message || 'Failed to fetch recent appeals'] };
    }
  }

  static async getAppealLocationsFromMultipleDistricts(districts) {
    try {
      if (!districts || districts.length === 0) return { success: true, appeals: [], errors: [] };

      const promises = districts.map(district =>
          ApiService.get(`/api/Appeal/map/${encodeURIComponent(district)}`).catch(error => {
            console.error(`Failed to fetch appeals for district ${district}:`, error);
            return [];
          })
      );

      const results = await Promise.all(promises);
      const appealsMap = new Map();

      results.forEach((appeals) => {
        if (Array.isArray(appeals)) {
          appeals.forEach(appeal => {
            const transformedAppeal = this.transformAppealToCamelCase(appeal);
            if (transformedAppeal.appealId) appealsMap.set(transformedAppeal.appealId, transformedAppeal);
          });
        }
      });

      return { success: true, appeals: Array.from(appealsMap.values()), errors: [] };
    } catch (error) {
      return { success: false, appeals: [], errors: [error.message || 'Failed to fetch appeals from multiple districts'] };
    }
  }

  static async getCategories() {
    try {
      const categories = await ApiService.get('/api/ProblemCategories/problem-categories');
      return { success: true, categories: categories || [] };
    } catch (error) {
      return { success: false, categories: [] };
    }
  }

  static async createAppeal(appealData) {
    try {
      const token = localStorage.getItem('accessToken');
      const formData = new FormData();

      if (appealData.categoryId) formData.append('CategoryId', appealData.categoryId);
      if (appealData.userId) formData.append('UserId', appealData.userId);
      if (appealData.title) formData.append('Title', appealData.title);
      if (appealData.description) formData.append('Description', appealData.description);
      formData.append('Latitude', appealData.latitude);
      formData.append('Longitude', appealData.longitude);

      if (appealData.city && appealData.city.trim() !== '') formData.append('City', appealData.city);
      if (appealData.street && appealData.street.trim() !== '') formData.append('Street', appealData.street);
      if (appealData.buildingNumber && appealData.buildingNumber.trim() !== '') formData.append('BuildingNumber', appealData.buildingNumber);

      if (appealData.photos && appealData.photos.length > 0) {
        for (let i = 0; i < appealData.photos.length; i++) {
          formData.append('Photos', appealData.photos[i]);
        }
      }

      const response = await fetch(`${API_BASE_URL}/api/Appeal`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
        body: formData,
      });

      if (response.ok) {
        return { success: true };
      } else {
        const errorData = await response.json();
        return { success: false, errors: errorData.errors || ['Failed to submit the appeal.'] };
      }
    } catch (error) {
      return { success: false, errors: [error.message] };
    }
  }

  /**
   * Fetch a specific appeal's simplified summary (Used by CityMap marker clicks)
   */
  static async getAppealSummary(appealId) {
    try {
      const rawResponse = await ApiService.get(`/api/Appeal/${encodeURIComponent(appealId)}`);

      // FIX: If backend returns an array for a single ID, extract the first item
      const response = Array.isArray(rawResponse) ? rawResponse[0] : rawResponse;

      if (!response) throw new Error("Appeal not found");

      const problemData = response.problemDto || response.ProblemDto || response;

      const mappedAppeal = {
        ...response,
        id: response.appealId || response.AppealId,
        title: problemData.title || problemData.Title || problemData.problemName || 'Infrastructure Problem',
        description: problemData.description || problemData.Description || 'No description provided.',
        status: response.status || response.Status || problemData.status || problemData.Status || 'PENDING',
        userFullName: response.userFullName || response.UserFullName || 'Citizen',
        date: response.createdAt || response.CreatedAt ? new Date(response.createdAt || response.CreatedAt).toLocaleDateString() : 'Unknown Date'
      };

      return { success: true, appeal: mappedAppeal, errors: [] };
    } catch (error) {
      return { success: false, appeal: null, errors: [error.message || `Failed to fetch appeal details for ID ${appealId}`] };
    }
  }

  /**
   * Fetch full appeal details including photos
   */
  static async getAppealDetails(appealId) {
    try {
      const rawResponse = await ApiService.get(`/api/Appeal/${encodeURIComponent(appealId)}`);

      // FIX: If backend returns an array for a single ID, extract the first item
      const response = Array.isArray(rawResponse) ? rawResponse[0] : rawResponse;

      if (!response) throw new Error("Appeal not found");

      const problemData = response.problemDto || response.ProblemDto || response;

      // Extract raw photos array
      const rawPhotos = problemData.photos || problemData.Photos || response.photos || response.Photos || [];

      // FIX: Pre-format photos into ready-to-use Data URIs based on the exact JSON structure
      const formattedPhotos = rawPhotos.map(photo => {
        if (typeof photo === 'string') {
          return photo.startsWith('http') ? photo : `data:image/jpeg;base64,${photo}`;
        }

        // Extract from the exact structure: { imageData: "...", contentType: "image/jpeg" }
        const base64 = photo.imageData || photo.ImageData;
        const mimeType = photo.contentType || photo.ContentType || 'image/jpeg';

        return base64 ? `data:${mimeType};base64,${base64}` : null;
      }).filter(p => p !== null); // Remove any failed image parses

      const fullAppeal = {
        id: response.appealId || response.AppealId,
        title: problemData.title || problemData.Title || problemData.problemName || 'Infrastructure Problem',
        description: problemData.description || problemData.Description || 'No description provided.',
        status: response.status || response.Status || problemData.status || problemData.Status || 'PENDING',
        userFullName: response.userFullName || response.UserFullName || 'Citizen',
        city: problemData.city || problemData.City || '',
        street: problemData.street || problemData.Street || '',
        buildingNumber: problemData.buildingNumber || problemData.BuildingNumber || '',
        latitude: response.latitude || response.Latitude || problemData.latitude || problemData.Latitude,
        longitude: response.longitude || response.Longitude || problemData.longitude || problemData.Longitude,
        date: response.createdAt || response.CreatedAt ? new Date(response.createdAt || response.CreatedAt).toLocaleString() : 'Unknown Date',
        photos: formattedPhotos
      };

      return { success: true, appeal: fullAppeal, errors: [] };
    } catch (error) {
      return { success: false, appeal: null, errors: [error.message || 'Failed to fetch full details.'] };
    }
  }
}

export default AppealService;