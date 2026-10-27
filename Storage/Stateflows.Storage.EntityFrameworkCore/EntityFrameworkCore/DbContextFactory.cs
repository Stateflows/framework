using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;

/// <summary>Creates Stateflows DbContext instances from the application's dependency-injection scope.</summary>
/// <typeparam name="TDbContext">The application's Entity Framework DbContext type.</typeparam>
/// <param name="serviceProvider">The service provider that resolves the DbContext.</param>
public class DbContextFactory<TDbContext>(IServiceProvider serviceProvider) : IDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    // private IServiceScope serviceScope = serviceProvider.CreateScope();
    
    /// <summary>Resolves a DbContext instance from the configured service provider.</summary>
    /// <returns>The registered <typeparamref name="TDbContext"/> instance.</returns>
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