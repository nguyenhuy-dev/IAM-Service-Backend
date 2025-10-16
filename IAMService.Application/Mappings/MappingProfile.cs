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
        }

    }
}
