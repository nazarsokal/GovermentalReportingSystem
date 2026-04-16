using Microsoft.AspNetCore.Http;
using ProblemReportingSystem.Application.DTOs;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

public interface ICityCouncilService
{
    Task<Guid> CreateCityCouncilAsync(CreateCityCouncilDto createCityCouncilDto);

    Task<IEnumerable<CityCouncilDto>> GetAllCityCouncilsAsync();

    Task<CityCouncilDetailsDto?> GetCityCouncilByIdAsync(Guid councilId);

    Task<IEnumerable<CityCouncilDto>> LoadCityCouncilsFromCsvAsync(IFormFile csvFile);
}