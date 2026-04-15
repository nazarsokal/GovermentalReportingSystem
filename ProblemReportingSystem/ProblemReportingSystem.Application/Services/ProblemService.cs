using AutoMapper;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.DAL.Entities;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

namespace ProblemReportingSystem.Application.Services;

public class ProblemService : IProblemService
{
    private readonly IProblemRepository _problemRepository;
    private readonly IGeolocateService _geolocateService;
    private readonly IMapper _mapper;

    public ProblemService(IProblemRepository problemRepository, IGeolocateService geolocateService ,IMapper mapper)
    {
        _problemRepository = problemRepository;
        _geolocateService = geolocateService;
        _mapper = mapper;
    }
    public async Task<Guid> CreateProblem(CreateProblemDto createProblemDto)
    {
        var problem = _mapper.Map<Problem>(createProblemDto);
        if(string.IsNullOrEmpty(problem.Address.City) || string.IsNullOrEmpty(problem.Address.Street))
        {
            var address = await _geolocateService.GetAddressFromCoordinatesAsync(problem.Address.Latitude, problem.Address.Longitude);
            problem.Address = _mapper.Map<Address>(address);
        }
        await _problemRepository.CreateAsync(problem);
        return problem.ProblemId;
    }

    public Task<Guid> UpdateProblem(UpdateProblemDto updateProblemDto)
    {
        throw new NotImplementedException();
    }
}