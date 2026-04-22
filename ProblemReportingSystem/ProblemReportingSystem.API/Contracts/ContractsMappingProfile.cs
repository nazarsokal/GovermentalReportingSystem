namespace ProblemReportingSystem.API.Contracts;

using AutoMapper;
using Request;
using Response;
using ProblemReportingSystem.Application.DTOs;

public class ContractsMappingProfile : Profile
{
    public ContractsMappingProfile()
    {
        // Appeal Request Mappings
        CreateMap<CreateAppealRequest, AppealDto>()
            .ForMember(dest => dest.AppealId, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)) // Map UserId from request
            .ForMember(dest => dest.ProblemDto, opt => opt.MapFrom(src => new CreateProblemDto
            {
                UserId = src.UserId,
                CategoryId = src.CategoryId,
                Title = src.Title,
                Description = src.Description,
                City = src.City,
                Street = src.Street,
                BuildingNumber = src.BuildingNumber,
                Latitude = src.Latitude,
                Longitude = src.Longitude,
                Photos = src.Photos != null
                    ? src.Photos.Select(file => new CreateProblemPhotoDto
                    {
                        ImageData = Array.Empty<byte>(), // Will be populated after file reading
                        ContentType = file.ContentType
                    }).ToList()
                    : new List<CreateProblemPhotoDto>()
            }))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        // Problem Request Mappings
        CreateMap<CreateAppealRequest, CreateProblemDto>()
            .ForMember(dest => dest.Photos, opt => opt.Ignore());

        CreateMap<UpdateProblemRequest, UpdateProblemDto>();

        // Problem Response Mappings
        CreateMap<ProblemDto, DetailProblemResponse>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => new ProblemCategoryResponse
            {
                CategoryId = src.CategoryId,
                Name = "Category",
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

        // Appeal Response Mappings
        CreateMap<AppealDto, SummaryAppealForMapResponse>()
            .ForMember(dest => dest.AppealId, opt => opt.MapFrom(src => src.AppealId))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.ProblemDto.Latitude))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.ProblemDto.Longitude))
            .ForMember(dest => dest.CategoryIconUrl, opt => opt.MapFrom(src => src.ProblemDto.CategoryId)); // Will be resolved from service
    }
}