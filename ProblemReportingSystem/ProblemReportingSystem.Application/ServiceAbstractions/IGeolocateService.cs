using ProblemReportingSystem.Application.DTOs;

namespace ProblemReportingSystem.Application.ServiceAbstractions;

public interface IGeolocateService
{
    public Task<ParsedAddressDto> GetAddressFromCoordinatesAsync(decimal latitude, decimal longitude);
}