using IAMService.Application.Interfaces;
using IAMService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IAMService.Infrastructure.Repositories
{
    public class UnitOfWork(IAMServiceDbContext context) : IUnitOfWork
    {
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
    }
}
