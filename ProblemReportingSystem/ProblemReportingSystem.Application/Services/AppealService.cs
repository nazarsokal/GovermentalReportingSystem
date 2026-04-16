using AutoMapper;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.Application.Services;

public class AppealService : IAppealService
{
    private readonly IAppealRepository _appealRepository;
    private readonly IMapper _mapper;
    private readonly IProblemService _problemService;

    public AppealService(IAppealRepository appealRepository, IMapper mapper, IProblemService problemService)
    {
        _appealRepository = appealRepository ?? throw new ArgumentNullException(nameof(appealRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _problemService = problemService;
    }

    public async Task<Guid> CreateAppealAsync(AppealDto createAppealDto)
    {
        var appeal = _mapper.Map<Appeal>(createAppealDto);
        var problem = await _problemService.CreateProblem(createAppealDto.ProblemDto);
        appeal.Problem = problem;
        
        await _appealRepository.CreateAsync(appeal);
        return appeal.AppealId;
    }

    public async Task<IEnumerable<AppealDto>> GetCouncilAppealsByEmployeeAsync(Guid employeeId)
    {
        var appeals = await _appealRepository.GetAppealsByEmployeeAsync(employeeId);
        return _mapper.Map<IEnumerable<AppealDto>>(appeals);
    }

    public async Task<IEnumerable<AppealDto>> GetAppealsByCouncilAsync(Guid councilId)
    {
        var appeals = await _appealRepository.GetAppealsByCouncilAsync(councilId);
        return _mapper.Map<IEnumerable<AppealDto>>(appeals);
    }

    public async Task<IEnumerable<AppealDto>> GetAppealsByStatusAsync(Guid councilId, string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be null or empty", nameof(status));

        var appeals = await _appealRepository.GetAppealsByStatusAsync(councilId, status);
        return _mapper.Map<IEnumerable<AppealDto>>(appeals);
    }

    public async Task<AppealDto?> UpdateAppealStatusAsync(Guid appealId, string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be null or empty", nameof(status));

        var appeal = await _appealRepository.GetByIdAsync(appealId);
        if (appeal == null)
            return null;

        // Problem is now nested within Appeal
        appeal.Problem.Status = status;
        appeal.UpdatedAt = DateTime.UtcNow;

        await _appealRepository.UpdateAsync(appeal);

        return _mapper.Map<AppealDto>(appeal);
    }

    public async Task<AppealDto?> AssignAppealAsync(Guid appealId, Guid employeeId)
    {
        var appeal = await _appealRepository.GetByIdAsync(appealId);
        if (appeal == null)
            return null;

        appeal.AssignedEmployeeId = employeeId;
        await _appealRepository.UpdateAsync(appeal);

        return _mapper.Map<AppealDto>(appeal);
    }
}
