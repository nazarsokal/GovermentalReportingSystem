using ProblemReportingSystem.Application.DTOs;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

public interface ICouncilEmployeeService
{
    Task<CouncilEmployeeDto?> GetEmployeeByUserIdAsync(Guid userId);

    Task<Guid?> GetCouncilIdByEmployeeIdAsync(Guid employeeId);

    Task<IEnumerable<CouncilEmployeeDto>> GetCouncilEmployeesAsync(Guid councilId);
}

