using AutoMapper;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.Application.Services;

public class CouncilEmployeeService : ICouncilEmployeeService
{
    private readonly ICouncilEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICityCouncilRepository _councilRepository;
    private readonly IMapper _mapper;

    public CouncilEmployeeService(
        ICouncilEmployeeRepository employeeRepository,
        IUserRepository userRepository,
        ICityCouncilRepository councilRepository,
        IMapper mapper)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _councilRepository = councilRepository ?? throw new ArgumentNullException(nameof(councilRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<CouncilEmployeeDto?> GetEmployeeByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var employee = await _employeeRepository.GetEmployeeByUserIdAsync(userId);
        return employee == null ? null : _mapper.Map<CouncilEmployeeDto>(employee);
    }

    public async Task<Guid?> GetCouncilIdByEmployeeIdAsync(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID cannot be empty", nameof(employeeId));

        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        return employee?.CouncilId;
    }

    public async Task<IEnumerable<CouncilEmployeeDto>> GetCouncilEmployeesAsync(Guid councilId)
    {
        if (councilId == Guid.Empty)
            throw new ArgumentException("Council ID cannot be empty", nameof(councilId));

        var employees = await _employeeRepository.GetCouncilEmployeesAsync(councilId);
        return _mapper.Map<IEnumerable<CouncilEmployeeDto>>(employees);
    }

    public async Task<IEnumerable<CouncilEmployeeDetailsDto>> GetCouncilEmployeesWithDetailsAsync(Guid councilId)
    {
        if (councilId == Guid.Empty)
            throw new ArgumentException("Council ID cannot be empty", nameof(councilId));

        var employees = await _employeeRepository.GetCouncilEmployeesAsync(councilId);
        return _mapper.Map<IEnumerable<CouncilEmployeeDetailsDto>>(employees);
    }

    public async Task<Guid> RegisterCouncilEmployeeAsync(Guid councilId, string fullName, string email, string password, string position)
    {
        if (councilId == Guid.Empty)
            throw new ArgumentException("Council ID cannot be empty", nameof(councilId));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required", nameof(fullName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required", nameof(password));
        if (string.IsNullOrWhiteSpace(position))
            throw new ArgumentException("Position is required", nameof(position));

        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        var council = await _councilRepository.GetByIdAsync(councilId);
        if (council == null)
            throw new InvalidOperationException("Council not found");

        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = fullName.Trim(),
            Email = email.Trim(),
            PasswordHash = AuthenticationService.HashPassword(password),
            // Use council address so employee is saved with council city/location.
            AddressId = council.AddressId,
            IsActive = true
        };

        await _userRepository.CreateAsync(user);

        var employee = new CouncilEmployee
        {
            EmployeeId = Guid.NewGuid(),
            CouncilId = councilId,
            UserId = user.UserId,
            Position = position.Trim()
        };

        await _employeeRepository.CreateAsync(employee);
        return employee.EmployeeId;
    }
}

