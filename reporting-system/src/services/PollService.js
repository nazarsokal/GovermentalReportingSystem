import ApiService from './ApiService';

class PollService {
  static async getDistrictPolls() {
    try {
      const userDistrict = localStorage.getItem('userDistrict');
      const query = userDistrict ? `?district=${encodeURIComponent(userDistrict)}` : '';
      const response = await ApiService.get(`/api/Polls/district-active${query}`);
      const polls = Array.isArray(response) ? response : [];
      return { success: true, polls };
    } catch (error) {
      return { success: false, polls: [], errors: [error.message || 'Failed to fetch polls'] };
    }
  }

  static async vote(pollId, optionId) {
    try {
      await ApiService.post(`/api/Polls/${pollId}/vote`, { optionId });
      return { success: true };
    } catch (error) {
      return { success: false, errors: [error.message || 'Failed to submit vote'] };
    }
  }
}

export default PollService;
