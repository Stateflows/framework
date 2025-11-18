using Microsoft.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Examples.Blazor;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IStateflowsDbContext_v1
{
    public DbSet<Context_v1> Contexts_v1 { get; set; }
    public DbSet<Notification_v1> Notifications_v1 { get; set; }
    public DbSet<Value_v1> Values_v1 { get; set; }
}