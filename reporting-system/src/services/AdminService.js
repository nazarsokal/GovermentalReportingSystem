import ApiService from './ApiService';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5216';

class AdminService {
    static async getUsers() {
        try {
            const response = await ApiService.get('/api/Admin/users');
            let usersArray = [];
            if (Array.isArray(response)) {
                usersArray = response;
            } else if (response && typeof response === 'object') {
                usersArray = response.data || response.users || response.items || Object.values(response).find(Array.isArray) || [];
            }
            return { success: true, users: usersArray };
        } catch (error) {
            return { success: false, users: [], errors: [error.message || 'Failed to fetch users'] };
        }
    }

    static async getCityCouncils() {
        try {
            const response = await ApiService.get('/api/Admin/city-councils');
            let councilsArray = [];
            if (Array.isArray(response)) {
                councilsArray = response;
            } else if (response && typeof response === 'object') {
                councilsArray = response.data || response.councils || response.items || Object.values(response).find(Array.isArray) || [];
            }
            return { success: true, councils: councilsArray };
        } catch (error) {
            return { success: false, councils: [], errors: [error.message || 'Failed to fetch city councils'] };
        }
    }

     /**
      * Create a new city council
      * POST /api/Admin/city-councils
      */
     static async createCityCouncil(councilData) {
         try {
             const token = localStorage.getItem('accessToken');
             const response = await fetch(`${API_BASE_URL}/api/Admin/city-councils`, {
                 method: 'POST',
                 headers: {
                     'Authorization': `Bearer ${token}`,
                     'Content-Type': 'application/json'
                 },
                 body: JSON.stringify(councilData)
             });

             if (response.ok) {
                 return { success: true };
             } else {
                 const errorData = await response.json().catch(() => ({}));
                 return { success: false, errors: errorData.errors || [errorData.message || 'Failed to create city council.'] };
             }
         } catch (error) {
             console.error('Failed to create city council:', error);
             return { success: false, errors: [error.message] };
         }
     }

     /**
      * Get a specific user by ID
      * GET /api/Admin/users/{userId}
      */
     static async getUserById(userId) {
         try {
             const response = await ApiService.get(`/api/Admin/users/${userId}`);
             return { success: true, user: response };
         } catch (error) {
             return { success: false, user: null, errors: [error.message || 'Failed to fetch user'] };
         }
     }

     /**
      * Create a new user
      * POST /api/Admin/users
      */
     static async createUser(userData) {
         try {
             const token = localStorage.getItem('accessToken');
             const response = await fetch(`${API_BASE_URL}/api/Admin/users`, {
                 method: 'POST',
                 headers: {
                     'Authorization': `Bearer ${token}`,
                     'Content-Type': 'application/json'
                 },
                 body: JSON.stringify(userData)
             });

             if (response.ok) {
                 return { success: true };
             } else {
                 const errorData = await response.json().catch(() => ({}));
                 return { success: false, errors: errorData.errors || [errorData.message || 'Failed to create user.'] };
             }
         } catch (error) {
             console.error('Failed to create user:', error);
             return { success: false, errors: [error.message] };
         }
     }

     /**
      * Update an existing user
      * PUT /api/Admin/users/{userId}
      */
     static async updateUser(userId, userData) {
         try {
             const response = await ApiService.put(`/api/Admin/users/${userId}`, userData);
             return { success: true };
         } catch (error) {
             return { success: false, errors: [error.message || 'Failed to update user.'] };
         }
     }

     /**
      * Delete a user
      * DELETE /api/Admin/users/{userId}
      */
     static async deleteUser(userId) {
         try {
             const response = await ApiService.delete(`/api/Admin/users/${userId}`);
             return { success: true };
         } catch (error) {
             return { success: false, errors: [error.message || 'Failed to delete user.'] };
         }
     }

     /**
      * Get a specific city council by ID
      * GET /api/Admin/city-councils/{councilId}
      */
     static async getCityCouncilById(councilId) {
         try {
             const response = await ApiService.get(`/api/Admin/city-councils/${councilId}`);
             return { success: true, council: response };
         } catch (error) {
             return { success: false, council: null, errors: [error.message || 'Failed to fetch city council'] };
         }
     }

     /**
      * Update an existing city council
      * PUT /api/Admin/city-councils/{councilId}
      */
     static async updateCityCouncil(councilId, councilData) {
         try {
             const response = await ApiService.put(`/api/Admin/city-councils/${councilId}`, councilData);
             return { success: true };
         } catch (error) {
             return { success: false, errors: [error.message || 'Failed to update city council.'] };
         }
     }

     /**
      * Delete a city council
      * DELETE /api/Admin/city-councils/{councilId}
      */
     static async deleteCityCouncil(councilId) {
         try {
             const response = await ApiService.delete(`/api/Admin/city-councils/${councilId}`);
             return { success: true };
         } catch (error) {
             return { success: false, errors: [error.message || 'Failed to delete city council.'] };
         }
     }

     /**
      * Upload appeals from CSV
      * POST /api/Appeal/upload-csv
      */
     static async uploadAppealsFromCSV(csvFile) {
         try {
             const token = localStorage.getItem('accessToken');
             const formData = new FormData();
             formData.append('CsvFile', csvFile);

             const response = await fetch(`${API_BASE_URL}/api/Appeal/upload-csv`, {
                 method: 'POST',
                 headers: {
                     'Authorization': `Bearer ${token}`
                 },
                 body: formData
             });

             const responseData = await response.json().catch(() => ({}));

             if (response.ok) {
                 return {
                     success: responseData.success ?? true,
                     message: responseData.message || 'Звернення успішно завантажено.',
                     result: responseData
                 };
             } else {
                 const errorMessages = Array.isArray(responseData.errors)
                     ? responseData.errors.map((error) => error?.errorMessage || error).filter(Boolean)
                     : [responseData.message || 'Failed to upload appeals from CSV.'];
                 return { success: false, errors: errorMessages };
             }
         } catch (error) {
             console.error('Failed to load CSV:', error);
             return { success: false, errors: [error.message] };
         }
     }
 }

 export default AdminService;
