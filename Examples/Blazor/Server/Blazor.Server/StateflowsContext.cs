using Microsoft.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Blazor.Server
{
    public class StateflowsContext : DbContext, IStateflowsDbContext_v1
    {
        public DbSet<Context_v1> Contexts_v1 { get; set; }

        public DbSet<TimeToken_v1> TimeTokens_v1 { get; set; }

        protected readonly IConfiguration Configuration;

        public StateflowsContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // connect to sql server with connection string from app settings
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("StateflowsDatabase"));
        }
    }
}
