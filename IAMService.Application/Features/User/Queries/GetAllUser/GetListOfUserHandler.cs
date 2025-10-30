using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Application.Interfaces;
using MediatR;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
namespace IAMService.Application.Features.User.Queries.GetAllUser
{
    /// <summary>
    /// Handles fetching and filtering users with pagination and sorting.
    /// </summary>
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IStringEncryptionService _stringEncryptionService;

        public GetUsersQueryHandler(IUserRepository userRepository, IMapper mapper, IStringEncryptionService stringEncryptionService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _stringEncryptionService = stringEncryptionService;
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


            var totalCount = await usersQueryable.CountAsync(cancellationToken);
            var users = await usersQueryable
                .AsNoTracking()
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);


            foreach (var user in users)
            {
                user.FullName = _stringEncryptionService.DecryptString(user.FullName);
                user.Email = _stringEncryptionService.DecryptString(user.Email);
                user.PhoneNumber = _stringEncryptionService.DecryptString(user.PhoneNumber);
                user.IdentityNumber = _stringEncryptionService.DecryptString(user.IdentityNumber);
                user.Address = _stringEncryptionService.DecryptString(user.Address);
            }


            var userDtos = _mapper.Map<List<UserDto>>(users);

            return new PaginatedList<UserDto>(
                userDtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );
        }
    }
}