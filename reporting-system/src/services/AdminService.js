import ApiService from './ApiService';

class AdminService {
    /**
     * Fetch all users for the admin dashboard
     */
    static async getUsers() {
        try {
            const response = await ApiService.get('/api/Admin/users');

            // FIX: Ensure we always return an array
            let usersArray = [];
            if (Array.isArray(response)) {
                usersArray = response;
            } else if (response && typeof response === 'object') {
                // Look for the array in common wrapper properties
                usersArray = response.data || response.users || response.items ||
                    Object.values(response).find(Array.isArray) || [];
            }

            return {
                success: true,
                users: usersArray
            };
        } catch (error) {
            console.error('Failed to fetch users:', error);
            return {
                success: false,
                users: [],
                errors: [error.message || 'Failed to fetch users']
            };
        }
    }

    /**
     * Fetch all city councils for the admin dashboard
     */
    static async getCityCouncils() {
        try {
            const response = await ApiService.get('/api/Admin/city-councils');

            // FIX: Ensure we always return an array
            let councilsArray = [];
            if (Array.isArray(response)) {
                councilsArray = response;
            } else if (response && typeof response === 'object') {
                // Look for the array in common wrapper properties
                councilsArray = response.data || response.councils || response.items ||
                    Object.values(response).find(Array.isArray) || [];
            }

            return {
                success: true,
                councils: councilsArray
            };
        } catch (error) {
            console.error('Failed to fetch city councils:', error);
            return {
                success: false,
                councils: [],
                errors: [error.message || 'Failed to fetch city councils']
            };
        }
    }
}

export default AdminService;