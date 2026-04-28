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
    public async Task<Problem> CreateProblem(CreateProblemDto createProblemDto)
    {
        var problem = _mapper.Map<Problem>(createProblemDto);
        
        // Ensure ProblemId is unique - generate new one if not set
        if (problem.ProblemId == Guid.Empty)
        {
            problem.ProblemId = Guid.NewGuid();
        }

        // Ensure Address has a unique ID
        if (problem.Address != null && problem.Address.AddressId == Guid.Empty)
        {
            problem.Address.AddressId = Guid.NewGuid();
        }
        
        // Assign optional address fields from DTO
        if (problem.Address != null)
        {
            if (!string.IsNullOrEmpty(createProblemDto.District))
                problem.Address.District = createProblemDto.District;
            if (!string.IsNullOrEmpty(createProblemDto.Oblast))
                problem.Address.Oblast = createProblemDto.Oblast;
            if (!string.IsNullOrEmpty(createProblemDto.Postcode))
                problem.Address.Postcode = createProblemDto.Postcode;
        }
        
        if(string.IsNullOrEmpty(problem.Address.City) || string.IsNullOrEmpty(problem.Address.Street))
        {
            var address = await _geolocateService.GetAddressFromCoordinatesAsync(problem.Address.Latitude , problem.Address.Longitude);
            problem.Address = _mapper.Map<Address>(address);
            
            // Ensure the new address also has a unique ID
            if (problem.Address.AddressId == Guid.Empty)
            {
                problem.Address.AddressId = Guid.NewGuid();
            }
            
            // Preserve user-provided optional fields from DTO
            if (!string.IsNullOrEmpty(createProblemDto.District))
                problem.Address.District = createProblemDto.District;
            if (!string.IsNullOrEmpty(createProblemDto.Oblast))
                problem.Address.Oblast = createProblemDto.Oblast;
            if (!string.IsNullOrEmpty(createProblemDto.Postcode))
                problem.Address.Postcode = createProblemDto.Postcode;
            
            problem.Address.Latitude = createProblemDto.Latitude;
            problem.Address.Longitude = createProblemDto.Longitude;
        }

        if (problem.ProblemPhotos != null && problem.ProblemPhotos.Any())
        {
            foreach (var photo in problem.ProblemPhotos)
            {
                // Ensure each photo has a unique ID
                if (photo.PhotoId == Guid.Empty)
                {
                    photo.PhotoId = Guid.NewGuid();
                }
                photo.ProblemId = problem.ProblemId;
            }
        }
        
        return problem;
    }

    public async Task<Guid> UpdateProblem(UpdateProblemDto updateProblemDto)
    {
        var problem = await _problemRepository.GetByIdAsync(updateProblemDto.ProblemId);
        if (problem == null)
            throw new NullReferenceException($"Problem with ID {updateProblemDto.ProblemId} not found");

        problem.Title = updateProblemDto.Title;
        problem.Description = updateProblemDto.Description;
        problem.Status = updateProblemDto.Status;

        await _problemRepository.UpdateAsync(problem);
        return problem.ProblemId;
    }

    public async Task<IEnumerable<ProblemDto>> GetAllProblems()
    {
        var problems = await _problemRepository.GetAllProblemsAsync();
        return _mapper.Map<IEnumerable<ProblemDto>>(problems);
    }

    public async Task<ProblemDto> GetProblemByIdAsync(Guid problemId)
    {
        var problem = await _problemRepository.GetProblemWithDetailsAsync(problemId);
        if (problem == null)
            throw new NullReferenceException("Problem not found"); 
        
        var problemDto = _mapper.Map<ProblemDto>(problem);
        return problemDto;
    }

    public async Task<IEnumerable<ProblemDto>> GetUserProblems(Guid userId)
    {
        var problems = await _problemRepository.GetUserProblemsAsync(userId);
        return _mapper.Map<IEnumerable<ProblemDto>>(problems);
    }

    public async Task<IEnumerable<ProblemDto>> GetProblemsByStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status cannot be null or empty", nameof(status));

        var problems = await _problemRepository.GetProblemsByStatusAsync(status);
        return _mapper.Map<IEnumerable<ProblemDto>>(problems);
    }

    public async Task<IEnumerable<ProblemDto>> GetProblemsByCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category ID cannot be empty", nameof(categoryId));

        var problems = await _problemRepository.GetProblemsByCategoryAsync(categoryId);
        return _mapper.Map<IEnumerable<ProblemDto>>(problems);
    }

    public async Task<IEnumerable<ProblemDto>> GetProblemsInBounds(decimal minLat, decimal maxLat, decimal minLng, decimal maxLng)
    {
        if (minLat > maxLat)
            throw new ArgumentException("Minimum latitude cannot be greater than maximum latitude");
        if (minLng > maxLng)
            throw new ArgumentException("Minimum longitude cannot be greater than maximum longitude");

        var problems = await _problemRepository.GetProblemsInBoundsAsync(minLat, maxLat, minLng, maxLng);
        return _mapper.Map<IEnumerable<ProblemDto>>(problems);
    }

    public async Task<IEnumerable<ProblemDto>> GetAssignedProblems(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID cannot be empty", nameof(employeeId));

        var problems = await _problemRepository.GetAssignedProblemsAsync(employeeId);
        return _mapper.Map<IEnumerable<ProblemDto>>(problems);
    }

}