import ApiService from './ApiService';

class CouncilService {
    /**
     * Fetch all appeals for a specific city council
     * GET /api/Appeal/council/{councilId}
     */
    static async getCouncilAppeals() {
        try {
            const response = await ApiService.get('/api/CouncilEmployee/appeals');
            const appeals = Array.isArray(response) ? response : [];
            const normalized = appeals.map((item) => {
                const problem = item.problem || {};
                return {
                    id: item.appealId || item.id,
                    appealId: item.appealId || item.id,
                    createdAt: item.createdAt,
                    updatedAt: item.updatedAt,
                    title: problem.title || 'Звернення',
                    description: problem.description || '',
                    status: problem.status || 'Pending',
                    address: [problem.oblast, problem.city, problem.street].filter(Boolean).join(', '),
                    assignedEmployeeId: item.assignedEmployeeId ?? null,
                    assignedEmployeeName: item.assignedEmployeeName ?? null
                };
            });

            return { success: true, data: normalized };
        } catch (error) {
            console.error('Failed to fetch council appeals:', error);
            return { success: false, data: [] };
        }
    }

    static async getMyAppeals() {
        try {
            const response = await ApiService.get('/api/CouncilEmployee/appeals/employee');
            const appeals = Array.isArray(response) ? response : [];
            const normalized = appeals.map((item) => {
                const problem = item.problem || {};
                return {
                    id: item.appealId || item.id,
                    appealId: item.appealId || item.id,
                    createdAt: item.createdAt,
                    updatedAt: item.updatedAt,
                    title: problem.title || 'Звернення',
                    description: problem.description || '',
                    status: problem.status || 'Pending',
                    address: [problem.oblast, problem.city, problem.street].filter(Boolean).join(', '),
                    assignedEmployeeId: item.assignedEmployeeId ?? null,
                    assignedEmployeeName: item.assignedEmployeeName ?? null
                };
            });

            return { success: true, data: normalized };
        } catch (error) {
            console.error('Failed to fetch my appeals:', error);
            return { success: false, data: [] };
        }
    }

    static async getCouncilEmployees() {
        try {
            const response = await ApiService.get('/api/CouncilEmployee/employees');
            const employees = Array.isArray(response) ? response : [];
            return { success: true, data: employees };
        } catch (error) {
            console.error('Failed to fetch council employees:', error);
            return { success: false, data: [] };
        }
    }

    static async assignAppeal(appealId, employeeId) {
        try {
            const response = await ApiService.put(`/api/CouncilEmployee/appeals/${appealId}/assign`, { employeeId });
            return { success: true, data: response };
        } catch (error) {
            return { success: false, errors: [error.message || 'Failed to assign appeal.'] };
        }
    }

    static async updateAppealStatus(appealId, status) {
        try {
            const response = await ApiService.patch(`/api/CouncilEmployee/appeals/${appealId}/status`, { status });
            return { success: true, data: response };
        } catch (error) {
            return { success: false, errors: [error.message || 'Failed to update appeal status.'] };
        }
    }

    /**
     * Fetch basic statistics for a specific council
     * GET /api/Appeal/statistics/council/{councilId}
     */
    static async getCouncilStats() {
        try {
            const response = await ApiService.get('/api/CouncilEmployee/statistics/kpi');
            return { success: true, data: response };
        } catch (error) {
            return { success: false, data: null };
        }
    }

    /**
     * Fetch efficiency report for current council (includes council name).
     * GET /api/CouncilEmployee/statistics/efficiency
     */
    static async getCouncilEfficiency() {
        try {
            const response = await ApiService.get('/api/CouncilEmployee/statistics/efficiency');
            return { success: true, data: response };
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
            return { success: true, data: response || {} };
        } catch (error) {
            return { success: false, data: {} };
        }
    }

    /**
     * Fetch general trends over time (For Line charts)
     * GET /api/Appeal/statistics/trends
     */
    static async getTrends() {
        try {
            const response = await ApiService.get('/api/Appeal/statistics/trends');
            if (Array.isArray(response)) {
                return { success: true, data: response };
            }

            if (response && typeof response === 'object') {
                return {
                    success: true,
                    data: [{
                        date: response.date ? new Date(response.date).toLocaleDateString('uk-UA') : 'Сьогодні',
                        count: response.resolvedCount ?? response.createdCount ?? 0
                    }]
                };
            }

            return { success: true, data: [] };
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
            const response = await ApiService.post('/api/CouncilEmployee/employees', employeeData);
            return { success: true, data: response };
        } catch (error) {
            return { success: false, errors: [error.message || 'Failed to register employee.'] };
        }
    }
}

export default CouncilService;