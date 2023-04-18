using System.Data.Entity;
using Stateflows.Storage.EntityFramework.EntityFramework.Entities;

namespace Stateflows.Storage.EntityFramework
{
    internal class StateflowsDbContext : DbContext
    {
        public DbSet<Context_v1> Contexts { get; set; }
        public DbSet<TimeToken_v1> TimeTokens { get; set; }
    }
}
