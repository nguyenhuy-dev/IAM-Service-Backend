using IAMService.Application.Interfaces;
using IAMService.Infrastructure.Data;
namespace IAMService.Infrastructure.Repositories
{
    /// <summary>
    ///     Implements the Unit of Work pattern, acting as a single transaction coordinator for database changes.
    ///     It wraps the underlying DbContext's save changes operation.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    public class UnitOfWork(IAMServiceDbContext context) : IUnitOfWork
    {
        /// <summary>
        ///     Persists all changes tracked by the DbContext to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation, returning the number of state entries written to the
        ///     database.
        /// </returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
    }
}
