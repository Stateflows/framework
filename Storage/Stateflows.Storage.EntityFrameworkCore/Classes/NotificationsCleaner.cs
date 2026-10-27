using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;

namespace Stateflows.Classes;

/// <summary>Runs a background cleanup loop for expired and superseded retained notifications.</summary>
/// <typeparam name="TDbContext">The Entity Framework context implementing the Stateflows storage contract.</typeparam>
public class NotificationsCleaner<TDbContext> : IHostedService
    where TDbContext : DbContext, IStateflowsDbContext_v1
{
    private Task cleaningTask = null;
    
    /// <summary>Creates the notification cleanup service.</summary>
    /// <param name="serviceProvider">The provider used to create a scoped DbContext for cleanup work.</param>
    public NotificationsCleaner(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    private readonly IServiceProvider ServiceProvider;
    private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
    
    /// <summary>Starts the periodic cleanup loop.</summary>
    /// <param name="cancellationToken">The hosted-service startup token; this implementation does not consume it.</param>
    /// <returns>A completed task after the cleanup loop is started.</returns>
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

                await using var scope = ServiceProvider.CreateAsyncScope();
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

    /// <summary>Cancels the cleanup loop and waits for it to stop.</summary>
    /// <param name="cancellationToken">The hosted-service shutdown token; the loop is stopped using its internal cancellation source.</param>
    /// <returns>A completed task after the loop has been synchronously awaited.</returns>
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