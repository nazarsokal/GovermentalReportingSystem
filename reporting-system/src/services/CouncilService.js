import ApiService from './ApiService';

class CouncilService {
    /**
     * Fetch all appeals for a specific city council
     * GET /api/Appeal/council/{councilId}
     */
    static async getCouncilAppeals(councilId) {
        try {
            const response = await ApiService.get(`/api/Appeal/council/${councilId}`);
            return { success: true, data: response.data || response || [] };
        } catch (error) {
            console.error('Failed to fetch council appeals:', error);
            return { success: false, data: [] };
        }
    }

    /**
     * Fetch basic statistics for a specific council
     * GET /api/Appeal/statistics/council/{councilId}
     */
    static async getCouncilStats(councilId) {
        try {
            const response = await ApiService.get(`/api/Appeal/statistics/council/${councilId}`);
            return { success: true, data: response.data || response };
        } catch (error) {
            return { success: false, data: null };
        }
    }

    /**
     * Fetch status distribution for a council (For Bar/Pie charts)
     * GET /api/Appeal/statistics/council/{councilId}/status-distribution
     */
    static async getStatusDistribution(councilId) {
        try {
            const response = await ApiService.get(`/api/Appeal/statistics/council/${councilId}/status-distribution`);
            return { success: true, data: response.data || response || [] };
        } catch (error) {
            return { success: false, data: [] };
        }
    }

    /**
     * Fetch general trends over time (For Line charts)
     * GET /api/Appeal/statistics/trends
     */
    static async getTrends() {
        try {
            const response = await ApiService.get('/api/Appeal/statistics/trends');
            return { success: true, data: response.data || response || [] };
        } catch (error) {
            return { success: false, data: [] };
        }
    }

    /**
     * Register a new council employee (Requires Head role)
     * Assuming standard Auth or Council endpoint for user creation
     */
    static async registerEmployee(employeeData) {
        try {
            // Adjust this URL to match your actual .NET endpoint for employee registration
            const response = await ApiService.post('/api/Auth/register-employee', employeeData);
            return { success: true, data: response };
        } catch (error) {
            return { success: false, errors: [error.message || 'Failed to register employee.'] };
        }
    }
}

export default CouncilService;