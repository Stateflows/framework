using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Tenant;

namespace Stateflows.Common.Scheduler
{
    internal class ThreadScheduler : IHostedService
    {
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private readonly IStateflowsTenantExecutor Executor;
        private readonly IServiceScope Scope;
        private readonly ILogger<ThreadScheduler> Logger;

        private IServiceProvider ServiceProvider
            => Scope.ServiceProvider;

        public ThreadScheduler(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();
            Executor = ServiceProvider.GetRequiredService<IStateflowsTenantExecutor>();
            Logger = ServiceProvider.GetRequiredService<ILogger<ThreadScheduler>>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() => TimingLoop(CancellationTokenSource.Token));

            return Task.CompletedTask;
        }

        private DateTime GetCurrentTick()
            => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

        private async Task TimingLoop(CancellationToken cancellationToken)
        {
            var lastTick = GetCurrentTick();

            while (!cancellationToken.IsCancellationRequested)
            {
                var diffInSeconds = (DateTime.Now - lastTick).TotalSeconds;

                if (diffInSeconds >= 60)
                {
                    lastTick = GetCurrentTick();

                    try
                    {
                        _ = Executor.ExecuteByTenantsAsync(() => HandleTimeEvents());
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(ThreadScheduler).FullName, nameof(TimingLoop), e.GetType().Name, e.Message);
                    }
                }

                await Task.Delay(1000);
            }
        }

        private async Task HandleTimeEvents()
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                try
                {
                    var runner = scope.ServiceProvider.GetRequiredService<ScheduleExecutor>();

                    await runner.ExecuteAsync();
                }
                catch (Exception e)
                {
                    Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(ThreadScheduler).FullName, nameof(HandleTimeEvents), e.GetType().Name, e.Message);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();

            Scope.Dispose();

            return Task.CompletedTask;
        }
    }
}