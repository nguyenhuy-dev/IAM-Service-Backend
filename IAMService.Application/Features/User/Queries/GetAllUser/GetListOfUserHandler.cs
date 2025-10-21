using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Application.Interfaces;
using MediatR;
using System.Linq.Expressions;
namespace IAMService.Application.Features.User.Queries.GetAllUser
{
    /// <summary>
    /// Handles fetching and filtering users with pagination and sorting.
    /// </summary>
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {

            var usersQueryable = _userRepository.GetUsersQueryable();


            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                string term = request.SearchTerm.ToLower();
                usersQueryable = usersQueryable.Where(u =>
                    u.FullName.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    u.PhoneNumber.Contains(term) ||
                    u.IdentityNumber.Contains(term) ||
                    u.Address.ToLower().Contains(term) ||
                    u.Role.RoleName.ToLower().Contains(term));
            }

            Expression<Func<IAMService.Domain.Entities.User, object>> keySelector = request.SortBy?.ToLower() switch
            {
                "email" => u => u.Email,
                "fullname" => u => u.FullName,
                "phonenumber" => u => u.PhoneNumber,
                "identitynumber" => u => u.IdentityNumber,
                "rolename" => u => u.Role.RoleName,
                _ => u => u.FullName,
            };

            usersQueryable = request.SortOrder?.ToLower() == "desc"
                ? usersQueryable.OrderByDescending(keySelector)
                : usersQueryable.OrderBy(keySelector);

            
            var dtoQueryable = _mapper.ProjectTo<UserDto>(usersQueryable);

            return await PaginatedList<UserDto>.CreateAsync(dtoQueryable, request.PageNumber, request.PageSize);
        }
    }
}
