namespace ProblemReportingSystem.Application.DTOs;

public class UserDto
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? GoogleAuthId { get; set; }

    public Guid? AddressId { get; set; }

    public bool? IsActive { get; set; }
}

public class CreateUserDto
{
    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;
    
    public string? Password { get; set; }

    public string? GoogleAuthId { get; set; }

    public string? City { get; set; }

    public Guid? AddressId { get; set; }
}

public class UpdateUserDto
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;
    
    public string? Password { get; set; }

    public string? GoogleAuthId { get; set; }

    public string? City { get; set; }

    public Guid? AddressId { get; set; }

    public bool? IsActive { get; set; }
}

public class UserDetailsDto : UserDto
{
    public AddressDto? Address { get; set; }

    public AdminDto? Admin { get; set; }

    public CouncilEmployeeDto? CouncilEmployee { get; set; }
}

public class UserWithRoleDto
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool? IsActive { get; set; }

    public Guid? AddressId { get; set; }
}
