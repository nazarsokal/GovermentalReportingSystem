using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.RepositoryAbstractions;
using Microsoft.Extensions.Logging;

namespace ProblemReportingSystem.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IUserRepository userRepository, 
        ILogger<AdminService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
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

    public async Task<Guid> CreateUserAsync(CreateUserDto createUserDto)
    {
        if (createUserDto == null)
            throw new ArgumentNullException(nameof(createUserDto));

        if (string.IsNullOrWhiteSpace(createUserDto.FullName))
            throw new ArgumentException("Full name is required", nameof(createUserDto.FullName));

        if (string.IsNullOrWhiteSpace(createUserDto.Email))
            throw new ArgumentException("Email is required", nameof(createUserDto.Email));

        // Check if user with email already exists
        var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
        if (existingUser != null)
            throw new InvalidOperationException($"User with email {createUserDto.Email} already exists");

        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = createUserDto.FullName,
            Email = createUserDto.Email,
            GoogleAuthId = createUserDto.GoogleAuthId,
            AddressId = createUserDto.AddressId,
            IsActive = true
        };

        // Hash password if provided
        if (!string.IsNullOrWhiteSpace(createUserDto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
        }

        await _userRepository.CreateAsync(user);
        
        _logger.LogInformation($"User created successfully with ID: {user.UserId}, Email: {user.Email}");
        return user.UserId;
    }

    public async Task<UserDto> UpdateUserAsync(UpdateUserDto updateUserDto)
    {
        if (updateUserDto == null)
            throw new ArgumentNullException(nameof(updateUserDto));

        if (updateUserDto.UserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(updateUserDto.UserId));

        if (string.IsNullOrWhiteSpace(updateUserDto.FullName))
            throw new ArgumentException("Full name is required", nameof(updateUserDto.FullName));

        if (string.IsNullOrWhiteSpace(updateUserDto.Email))
            throw new ArgumentException("Email is required", nameof(updateUserDto.Email));

        var user = await _userRepository.GetByIdAsync(updateUserDto.UserId);
        if (user == null)
            throw new InvalidOperationException($"User with ID {updateUserDto.UserId} not found");

        // Check if new email is already taken by another user
        if (!user.Email.Equals(updateUserDto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userRepository.GetByEmailAsync(updateUserDto.Email);
            if (existingUser != null)
                throw new InvalidOperationException($"User with email {updateUserDto.Email} already exists");
        }

        user.FullName = updateUserDto.FullName;
        user.Email = updateUserDto.Email;
        user.GoogleAuthId = updateUserDto.GoogleAuthId;
        user.AddressId = updateUserDto.AddressId;
        user.IsActive = updateUserDto.IsActive ?? user.IsActive;

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
        }

        await _userRepository.UpdateAsync(user);
        
        _logger.LogInformation($"User updated successfully with ID: {user.UserId}");

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

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found for deletion");
            return false;
        }

        await _userRepository.DeleteAsync(user);
        
        _logger.LogInformation($"User deleted successfully with ID: {userId}");
        return true;
    }

    /// <summary>
    /// Determines the role of a user based on their entity relationships
    /// </summary>
    private static string DetermineRole(User user)
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