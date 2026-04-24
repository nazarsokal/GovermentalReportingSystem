using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly ICityCouncilService _cityCouncilService;

    public AdminService(IUserRepository userRepository, ICityCouncilService cityCouncilService)
    {
        _userRepository = userRepository;
        _cityCouncilService = cityCouncilService;
    }
    
    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null)
            return null!;

        return new UserDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            GoogleAuthId = user.GoogleAuthId,
            AddressId = user.AddressId,
            IsActive = user.IsActive
        };
    }

    public async Task<IEnumerable<UserWithRoleDto>> GetUsersAsync()
    {
        var users = _userRepository.GetAll().ToList();
        var usersDto = users.Select(u => new UserWithRoleDto
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            Role = DetermineRole(u),
            IsActive = u.IsActive,
            AddressId = u.AddressId
        }).ToList();
        
        return await Task.FromResult(usersDto);
    }

    public async Task<Guid> CreateCityCouncilAsync(CreateCityCouncilDto createCityCouncilDto)
    {
        return await _cityCouncilService.CreateCityCouncilAsync(createCityCouncilDto);
    }

    /// <summary>
    /// Determines the role of a user based on their entity relationships
    /// </summary>
    private static string DetermineRole(DAL.Entities.User user)
    {
        if (user.Admin != null)
        {
            return "Admin";
        }

        if (user.CouncilEmployee != null)
        {
            return "CouncilEmployee";
        }

        return "User";
    }
}