using ProblemReportingSystem.Application.DTOs;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

public interface IAdminService
{
    // User Management
    Task<UserDto> GetUserByIdAsync(Guid userId);
    
    Task<IEnumerable<UserWithRoleDto>> GetUsersAsync();

    Task<Guid> CreateUserAsync(CreateUserDto createUserDto);

    Task<UserDto> UpdateUserAsync(UpdateUserDto updateUserDto);

    Task<bool> DeleteUserAsync(Guid userId);
}