using Microsoft.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore
{
#pragma warning disable S101 // Types should be named in PascalCase
    public interface IStateflowsDbContext_v1
#pragma warning restore S101 // Types should be named in PascalCase
    {
        DbSet<Context_v1> Contexts_v1 { get; set; }
        DbSet<Notification_v1> Notifications_v1 { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
