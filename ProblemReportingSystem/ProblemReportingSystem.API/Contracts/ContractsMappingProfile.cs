namespace ProblemReportingSystem.API.Contracts;

using AutoMapper;
using Request;
using ProblemReportingSystem.Application.DTOs;

public class ContractsMappingProfile : Profile
{
    public ContractsMappingProfile()
    {
        // Problem Request Mappings
        CreateMap<CreateProblemRequest, CreateProblemDto>()
            .ForMember(dest => dest.Photos, opt => opt.Ignore());; // AddressId will be set separately after Address creation
    }
}