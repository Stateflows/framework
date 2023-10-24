using Microsoft.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore
{
    public interface IStateflowsDbContext_v1
    {
        DbSet<Context_v1> Contexts_v1 { get; set; }
        DbSet<TimeToken_v1> TimeTokens_v1 { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
