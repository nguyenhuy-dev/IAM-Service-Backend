using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Application.IntegrationEvents;
using IAMService.Domain.Entities;
namespace IAMService.Application.Mappings
{
    /// <summary>
    ///     The mapping profile class
    /// </summary>
    /// <seealso cref="Profile" />
    public class MappingProfile : Profile
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MappingProfile" /> class
        /// </summary>
        public MappingProfile()
        {
            CreateMap<Privilege, PrivilegeDto>();
            CreateMap<Role, GetRoleRequest>();
            CreateMap<Role, RoleDto>();
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Gender,
                    opt => opt.MapFrom(src => src.Gender ? "Male" : "Female"))
                .ForMember(dest => dest.GeneratedPassword,
                    opt => opt.Ignore());

            // Add mapping for View Information User
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.RoleName,
                    opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : null))
                .ForMember(dest => dest.Age,
                    opt => opt.MapFrom(src => src.Age))
                .ForMember(dest => dest.PrivilegeIds,
                    opt => opt.MapFrom(src => src.Role != null
                        ? src.Role.Privileges.Select(p => p.PrivilegeId).ToList()
                        : new List<int>()))
                .ForMember(dest => dest.PrivilegeNames,
                    opt => opt.MapFrom(src => src.Role != null
                        ? src.Role.Privileges.Select(p => p.PrivilegeName).ToList()
                        : new List<string>()));

            CreateMap<UpdateUserRequestDto, UserUpdatedIsPatientIntegrationEvent>()
                .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore())
                .ForMember(dest => dest.EventId, opt => opt.Ignore())
                .ForMember(dest => dest.EventCreationDate, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForAllMembers(options =>
                    options.Condition((src, dest, srcMember) => srcMember != null)
                );

            CreateMap<User, UserUpdatedIsPatientIntegrationEvent>()
                .ForMember(dest => dest.EventId, opt => opt.Ignore())
                .ForMember(dest => dest.EventCreationDate, opt => opt.Ignore());

            CreateMap<User, User>();
        }
    }
}
