using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Domain.Entities;

namespace IAMService.Application.Mappings
{
    /// <summary>
    /// The mapping profile class
    /// </summary>
    /// <seealso cref="Profile"/>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProfile"/> class
        /// </summary>
        public MappingProfile()
        {
            CreateMap<Privilege, PrivilegeDto>();
            CreateMap<Role, GetRoleRequest>();
            CreateMap<Role, RoleDto>();

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
        }

    }
}
