using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;

namespace Stateflows.Classes;

public class NotificationsCleaner<TDbContext> : IHostedService
    where TDbContext : DbContext, IStateflowsDbContext_v1
{
    private Task cleaningTask = null;
    
    public NotificationsCleaner(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    private readonly IServiceProvider ServiceProvider;
    private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        cleaningTask = CleaningLoopAsync(CancellationTokenSource.Token);

        return Task.CompletedTask;
    }
    
    private static DateTime GetCurrentTick()
        => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
    
    private async Task CleaningLoopAsync(CancellationToken cancellationToken)
    {
        var lastTick = GetCurrentTick();
    
        while (!cancellationToken.IsCancellationRequested)
        {
            var diffInSeconds = (DateTime.Now - lastTick).TotalSeconds;

            if (diffInSeconds >= 60)
            {
                lastTick = GetCurrentTick();

                var date = DateTime.Now.AddMinutes(-1);

                using var scope = ServiceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

                await context.Notifications_v1.Where(n =>
                    n.SentAt.AddSeconds(n.TimeToLive) < date &&
                    !n.Retained
                ).ExecuteDeleteAsync(cancellationToken);
                
                // Remove older retained entries, keeping only the newest retained per group.
                await context.Notifications_v1.Where(n =>
                    n.Retained &&
                    context.Notifications_v1.Any(m =>
                        m.Retained &&
                        m.SenderType == n.SenderType &&
                        m.SenderName == n.SenderName &&
                        m.SenderInstance == n.SenderInstance &&
                        m.Name == n.Name &&
                        m.SentAt > n.SentAt
                    )
                ).ExecuteDeleteAsync(cancellationToken);
            }
    
            await Task.Delay(1000, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            CancellationTokenSource.Cancel();

            cleaningTask.Wait();
        }
        catch (Exception)
        {
        }

        return Task.CompletedTask;
    }
}