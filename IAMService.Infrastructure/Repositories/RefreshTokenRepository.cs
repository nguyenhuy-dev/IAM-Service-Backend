using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;
using IAMService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IAMService.Infrastructure.Repositories
{
    /// <summary>
    /// Implements the repository for managing <see cref="RefreshToken"/> entities in the database.
    /// Provides CRUD operations and specific queries related to token status.
    /// </summary>
    /// <seealso cref="IRefreshTokenRepository" />
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        /// <summary>
        /// The database context.
        /// </summary>
        private readonly IAMServiceDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenRepository"/> class.
        /// </summary>
        /// <param name="context">The database context (<c>IAMServiceDbContext</c>) injected via Dependency Injection.</param>
        public RefreshTokenRepository(IAMServiceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new <see cref="RefreshToken"/> asynchronously to the database.
        /// (NOTE: SaveChanges is removed here to align with the Unit of Work pattern in the Application layer.)
        /// </summary>
        /// <param name="refreshToken">The refresh token object to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning the added <see cref="RefreshToken"/>.</returns>
        public async Task<RefreshToken> AddAsync(RefreshToken refreshToken)
        {
            // XÓA: await _context.SaveChangesAsync(); để UoW kiểm soát
            _context.RefreshTokens.Add(refreshToken);
            return await Task.FromResult(refreshToken); // Trả về entity sau khi thêm vào context
        }

        /// <summary>
        /// Cleans up and removes all expired refresh tokens (where <c>ExpiresAt</c> &lt;= <c>DateTime.UtcNow</c>) from the database asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning the number of tokens deleted.</returns>
        public async Task<int> CleanupExpiredTokenAsync()
        {
            // Đây là một hàm đơn lẻ, việc gọi SaveChanges là hợp lý để thực thi ngay lệnh dọn dẹp.

            // Sử dụng ExecuteDeleteAsync để tối ưu hóa việc xóa hàng loạt trong EF Core
            int tokensDeleted = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
                .ExecuteDeleteAsync(); // Yêu cầu EF Core 7+

            // Nếu không dùng ExecuteDeleteAsync, giữ lại logic cũ:
            /*
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
            _context.RefreshTokens.RemoveRange(expiredTokens);
            int tokensDeleted = await _context.SaveChangesAsync();
            */

            return tokensDeleted;
        }

        /// <summary>
        /// Gets a list of all active refresh tokens for a specific user asynchronously.
        /// An active token is one that has not been revoked (<c>RevokedAt</c> is null) and has not expired.
        /// </summary>
        /// <param name="userId">The ID of the user (Guid).</param> // SỬA: Kiểu dữ liệu thành Guid
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning a <c>List&lt;RefreshToken&gt;</c>.</returns>
        public async Task<List<RefreshToken>> GetActiveTokenByUserIdAsync(Guid userId) // SỬA: Kiểu dữ liệu thành Guid
        {
            // SỬA: So sánh với userId kiểu Guid
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a <see cref="RefreshToken"/> by its ID asynchronously.
        /// </summary>
        /// <param name="tokenId">The ID of the token (Guid).</param> // SỬA: Kiểu dữ liệu thành Guid
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning the <see cref="RefreshToken"/> or null if not found.</returns>
        public async Task<RefreshToken?> GetByIdAsync(Guid tokenId) // SỬA: Kiểu dữ liệu thành Guid
        {
            // SỬA: FindAsync phải được gọi với kiểu dữ liệu khóa chính là Guid
            return await _context.RefreshTokens.FindAsync(tokenId);
        }

        /// <summary>
        /// Gets an active <see cref="RefreshToken"/> by its hash value asynchronously.
        /// It only returns tokens that are not revoked and have not expired.
        /// </summary>
        /// <param name="tokenHash">The hashed value of the token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning the active <see cref="RefreshToken"/> or null if not found/inactive.</returns>
        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash
                                             && rt.RevokedAt == null
                                             && rt.ExpiresAt > DateTime.UtcNow);
        }

        /// <summary>
        /// Updates an existing <see cref="RefreshToken"/> object in the database asynchronously.
        /// (NOTE: SaveChanges is removed here to align with the Unit of Work pattern in the Application layer.)
        /// </summary>
        /// <param name="refreshToken">The modified <see cref="RefreshToken"/> object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning <c>true</c> if the update was tracked, otherwise <c>false</c>.</returns>
        public async Task<bool> UpdateAsync(RefreshToken refreshToken)
        {
            // XÓA: int affectedRows = await _context.SaveChangesAsync();

            // The Update method marks the entity as modified. 
            _context.RefreshTokens.Update(refreshToken);

            // XÓA: return affectedRows > 0;
            // Thay vào đó, Repository chỉ cần báo entity đã sẵn sàng được lưu bởi UoW.
            return await Task.FromResult(true);
        }
    }
}