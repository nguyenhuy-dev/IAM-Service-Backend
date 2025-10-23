using System.Threading;
using System.Threading.Tasks;

namespace IAMService.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for the Unit of Work pattern, coordinating the saving of
    /// changes for all repositories within a single business transaction.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Persists all changes tracked by the context to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation, returning the number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}