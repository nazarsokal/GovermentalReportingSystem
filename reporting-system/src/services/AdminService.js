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
}

export default AdminService;