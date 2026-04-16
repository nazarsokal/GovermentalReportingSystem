using AutoMapper;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.Application.Services;

public class CouncilEmployeeService : ICouncilEmployeeService
{
    private readonly ICouncilEmployeeRepository _employeeRepository;
    private readonly IMapper _mapper;

    public CouncilEmployeeService(ICouncilEmployeeRepository employeeRepository, IMapper mapper)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
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
}

