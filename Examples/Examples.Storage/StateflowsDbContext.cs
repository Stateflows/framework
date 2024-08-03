using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Examples.Storage
{
    public class StateflowsDbContext : DbContext, IStateflowsDbContext_v1
    {
        public DbSet<Context_v1> Contexts_v1 { get; set; }

        protected readonly IConfiguration Configuration;

        public StateflowsDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // connect to sql server with connection string from app settings
            optionsBuilder.UseSqlServer(
                Configuration.GetConnectionString("StateflowsDatabase")
            );
        }
    }
}
