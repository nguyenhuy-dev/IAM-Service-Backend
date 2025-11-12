using AutoMapper;
using IAMService.Application.DTOs;
using IAMService.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace IAMService.Application.Features.User.Queries.GetAllUser
{
    /// <summary>
    ///     Handles fetching and filtering users with pagination and sorting.
    /// </summary>
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
    {
        private readonly IMapper _mapper;
        private readonly IStringEncryptionService _stringEncryptionService;
        private readonly IUserRepository _userRepository;

        public GetUsersQueryHandler(IUserRepository userRepository, IMapper mapper, IStringEncryptionService stringEncryptionService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _stringEncryptionService = stringEncryptionService;
        }

        public async Task<PaginatedList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var usersQueryable = _userRepository.GetUsersQueryable();

            // Lấy tất cả user trước (vì dữ liệu trong DB là mã hóa)
            var users = await usersQueryable
                .Include(u => u.Role)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Decrypt toàn bộ
            foreach (var user in users)
            {
                user.FullName = _stringEncryptionService.DecryptString(user.FullName);
                user.Email = _stringEncryptionService.DecryptString(user.Email);
                user.PhoneNumber = _stringEncryptionService.DecryptString(user.PhoneNumber);
                user.IdentityNumber = _stringEncryptionService.DecryptString(user.IdentityNumber);
                user.Address = _stringEncryptionService.DecryptString(user.Address);
            }

            // Search sau khi decrypt
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();

                users = users.Where(u =>
                    (u.FullName ?? "").ToLower().Contains(term) ||
                    (u.Email ?? "").ToLower().Contains(term) ||
                    (u.PhoneNumber ?? "").ToLower().Contains(term) ||
                    (u.IdentityNumber ?? "").ToLower().Contains(term) ||
                    (u.Address ?? "").ToLower().Contains(term) ||
                    ((u.Role != null ? u.Role.RoleName : "") ?? "").ToLower().Contains(term)
                ).ToList();
            }

            // Sort
            Func<Domain.Entities.User, object> keySelector = request.SortBy?.ToLower() switch
            {
                "email" => u => u.Email,
                "fullname" => u => u.FullName,
                "phonenumber" => u => u.PhoneNumber,
                "identitynumber" => u => u.IdentityNumber,
                "rolename" => u => u.Role?.RoleName ?? "",
                _ => u => u.FullName
            };

            users = request.SortOrder?.ToLower() == "desc"
                ? users.OrderByDescending(keySelector).ToList()
                : users.OrderBy(keySelector).ToList();

            // Pagination
            var totalCount = users.Count;
            var paginatedUsers = users
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Mapping sang DTO
            var userDtos = _mapper.Map<List<UserDto>>(paginatedUsers);

            return new PaginatedList<UserDto>(
                userDtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );
        }
    }
}
