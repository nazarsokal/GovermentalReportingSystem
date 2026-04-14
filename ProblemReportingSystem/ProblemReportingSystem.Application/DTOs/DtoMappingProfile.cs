namespace ProblemReportingSystem.Application.DTOs;

using AutoMapper;
using ProblemReportingSystem.DAL.Entities;

public class DtoMappingProfile : Profile
{
    public DtoMappingProfile()
    {
        // Address Mappings
        CreateMap<Address, AddressDto>().ReverseMap();
        CreateMap<CreateAddressDto, Address>();
        CreateMap<UpdateAddressDto, Address>();

        // User Mappings
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
        CreateMap<User, UserDetailsDto>()
            .ForMember(dest => dest.Admin, opt => opt.MapFrom(src => src.Admin))
            .ForMember(dest => dest.CouncilEmployee, opt => opt.MapFrom(src => src.CouncilEmployee));

        // Admin Mappings
        CreateMap<Admin, AdminDto>().ReverseMap();
        CreateMap<CreateAdminDto, Admin>();
        CreateMap<UpdateAdminDto, Admin>();
        CreateMap<Admin, AdminDetailsDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

        // ProblemCategory Mappings
        CreateMap<ProblemCategory, ProblemCategoryDto>().ReverseMap();
        CreateMap<CreateProblemCategoryDto, ProblemCategory>();
        CreateMap<UpdateProblemCategoryDto, ProblemCategory>();

        // ProblemPhoto Mappings
        CreateMap<ProblemPhoto, ProblemPhotoDto>().ReverseMap();
        CreateMap<CreateProblemPhotoDto, ProblemPhoto>();
        CreateMap<UpdateProblemPhotoDto, ProblemPhoto>();

        // Problem Mappings
        CreateMap<Problem, ProblemDto>().ReverseMap();
        CreateMap<CreateProblemDto, Problem>();
        CreateMap<UpdateProblemDto, Problem>();
        CreateMap<Problem, ProblemDetailsDto>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.ProblemPhotos, opt => opt.MapFrom(src => src.ProblemPhotos));

        // Appeal Mappings
        CreateMap<Appeal, AppealDto>().ReverseMap();
        CreateMap<CreateAppealDto, Appeal>();
        CreateMap<UpdateAppealDto, Appeal>();
        CreateMap<Appeal, AppealDetailsDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.Problem, opt => opt.MapFrom(src => src.Problem))
            .ForMember(dest => dest.AssignedEmployee, opt => opt.MapFrom(src => src.AssignedEmployee));

        // PollOption Mappings
        CreateMap<PollOption, PollOptionDto>().ReverseMap();
        CreateMap<CreatePollOptionDto, PollOption>();
        CreateMap<UpdatePollOptionDto, PollOption>();
        CreateMap<PollOption, PollOptionDetailsDto>()
            .ForMember(dest => dest.VoteCount, opt => opt.MapFrom(src => src.PollVotes.Count));

        // Poll Mappings
        CreateMap<Poll, PollDto>().ReverseMap();
        CreateMap<CreatePollDto, Poll>();
        CreateMap<UpdatePollDto, Poll>();
        CreateMap<Poll, PollDetailsDto>()
            .ForMember(dest => dest.Council, opt => opt.MapFrom(src => src.Council))
            .ForMember(dest => dest.PollOptions, opt => opt.MapFrom(src => src.PollOptions));

        // PollVote Mappings
        CreateMap<PollVote, PollVoteDto>().ReverseMap();
        CreateMap<CreatePollVoteDto, PollVote>();
        CreateMap<PollVote, PollVoteDetailsDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.Option, opt => opt.MapFrom(src => src.Option));

        // CityCouncil Mappings
        CreateMap<CityCouncil, CityCouncilDto>().ReverseMap();
        CreateMap<CreateCityCouncilDto, CityCouncil>();
        CreateMap<UpdateCityCouncilDto, CityCouncil>();
        CreateMap<CityCouncil, CityCouncilDetailsDto>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.CouncilEmployees, opt => opt.MapFrom(src => src.CouncilEmployees))
            .ForMember(dest => dest.Polls, opt => opt.MapFrom(src => src.Polls));

        // CouncilEmployee Mappings
        CreateMap<CouncilEmployee, CouncilEmployeeDto>().ReverseMap();
        CreateMap<CreateCouncilEmployeeDto, CouncilEmployee>();
        CreateMap<UpdateCouncilEmployeeDto, CouncilEmployee>();
        CreateMap<CouncilEmployee, CouncilEmployeeDetailsDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.Council, opt => opt.MapFrom(src => src.Council));

        // CouncilEfficiencyReport Mappings
        CreateMap<VCouncilEfficiencyReport, CouncilEfficiencyReportDto>().ReverseMap();
    }
}