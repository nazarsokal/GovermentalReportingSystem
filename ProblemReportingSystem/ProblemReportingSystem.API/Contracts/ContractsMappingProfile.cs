namespace ProblemReportingSystem.API.Contracts;

using AutoMapper;
using Request;
using Response;
using ProblemReportingSystem.Application.DTOs;

public class ContractsMappingProfile : Profile
{
    public ContractsMappingProfile()
    {
        // Problem Request Mappings
        CreateMap<CreateProblemRequest, CreateProblemDto>()
            .ForMember(dest => dest.Photos, opt => opt.Ignore());; // AddressId will be set separately after Address creation

        CreateMap<UpdateProblemRequest, UpdateProblemDto>();

        // Problem Response Mappings
        CreateMap<ProblemDto, DetailProblemResponse>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => new ProblemCategoryResponse
            {
                CategoryId = src.CategoryId,
                Name = src.Address != null ? "Category" : "Unknown",
                Description = null
            }))
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos));

        // Problem Summary Response Mapping
        CreateMap<ProblemDto, SummaryProblemResponse>()
            .ForMember(dest => dest.ProblemId, opt => opt.MapFrom(src => src.ProblemId))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
            .ForMember(dest => dest.Oblast, opt => opt.MapFrom(src => src.Address.Oblast))
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street));

        // Address Response Mapping
        CreateMap<AddressDto, AddressResponse>();

        // Problem Photo Response Mapping
        CreateMap<ProblemPhotoDto, ProblemPhotoResponse>();
    }
}