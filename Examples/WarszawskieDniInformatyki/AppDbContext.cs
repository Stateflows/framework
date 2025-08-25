using Microsoft.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace WarszawskieDniInformatyki;

public class AppDbContext : DbContext, IStateflowsDbContext_v1 
{
    
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Context_v1> Contexts_v1 { get; set; }
    public DbSet<Notification_v1> Notifications_v1 { get; set; }
}