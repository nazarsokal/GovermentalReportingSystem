using ProblemReportingSystem.Application.DTOs;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

public interface IAdminService
{
    public Task<UserDto> GetUserByIdAsync(Guid userId);
    
    public Task<IEnumerable<UserWithRoleDto>> GetUsersAsync();

    public Task<Guid> CreateCityCouncilAsync(CreateCityCouncilDto createCityCouncilDto);
}