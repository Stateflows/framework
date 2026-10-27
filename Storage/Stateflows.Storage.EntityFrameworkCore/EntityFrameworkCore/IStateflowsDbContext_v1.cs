using Microsoft.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore
{
#pragma warning disable S101 // Types should be named in PascalCase
    /// <summary>Exposes the Entity Framework sets required by the version 1 Stateflows storage providers.</summary>
    /// <remarks>Implement this contract on the application's registered DbContext and include its sets in the EF model.</remarks>
    public interface IStateflowsDbContext_v1 : IDisposable
#pragma warning restore S101 // Types should be named in PascalCase
    {
        /// <summary>Gets or sets the persisted behavior execution contexts.</summary>
        DbSet<Context_v1> Contexts_v1 { get; set; }
        /// <summary>Gets or sets the persisted notifications.</summary>
        DbSet<Notification_v1> Notifications_v1 { get; set; }
        /// <summary>Gets or sets the persisted behavior values.</summary>
        DbSet<Value_v1> Values_v1 { get; set; }
        /// <summary>Saves the pending changes tracked by the DbContext.</summary>
        /// <param name="cancellationToken">A token that can cancel the save operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
