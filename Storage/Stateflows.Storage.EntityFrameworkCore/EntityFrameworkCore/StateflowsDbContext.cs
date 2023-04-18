using Microsoft.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore
{
    internal class StateflowsDbContext : DbContext
    {
        public StateflowsDbContext(DbContextOptions<StateflowsDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Context_v1> Contexts_v1 { get; set; }
        public DbSet<TimeToken_v1> TimeTokens_v1 { get; set; }
    }
}
