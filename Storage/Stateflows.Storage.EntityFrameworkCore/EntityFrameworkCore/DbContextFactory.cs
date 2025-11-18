using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;

public class DbContextFactory<TDbContext>(IServiceProvider serviceProvider) : IDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    // private IServiceScope serviceScope = serviceProvider.CreateScope();
    
    public TDbContext CreateDbContext()
        => serviceProvider.GetRequiredService<TDbContext>();

    // public void Dispose()
    // {
    //     try
    //     {
    //         serviceScope.Dispose();
    //         serviceScope = serviceProvider.CreateScope();
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         throw;
    //     }
    // }
}