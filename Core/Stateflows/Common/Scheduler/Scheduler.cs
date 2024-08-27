using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Initializer;
using System.Linq;

namespace Stateflows.Common.Scheduler
{
    internal class Scheduler : IHostedService
    {
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private readonly IStateflowsTenantExecutor Executor;
        private readonly IServiceScope Scope;
        private readonly ILogger<Scheduler> Logger;
        private readonly IBehaviorLocator Locator;
        private readonly IHostApplicationLifetime Lifetime;

        private IServiceProvider ServiceProvider
            => Scope.ServiceProvider;

        public Scheduler(IServiceProvider serviceProvider, IHostApplicationLifetime lifetime)
        {
            Scope = serviceProvider.CreateScope();
            Executor = ServiceProvider.GetRequiredService<IStateflowsTenantExecutor>();
            Logger = ServiceProvider.GetRequiredService<ILogger<Scheduler>>();
            Locator = ServiceProvider.GetRequiredService<IBehaviorLocator>();
            Lifetime = lifetime;
        }

        public void ApplicationStarted()
        {
            Task.WaitAll(
                Executor.ExecuteByTenantsAsync(() => InitiateBehaviors()),
                Executor.ExecuteByTenantsAsync(() => HandleStartupEvents())
            );
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Lifetime.ApplicationStarted.Register(ApplicationStarted);

            _ = Task.Run(async () =>
            {
                await TimingLoop(CancellationTokenSource.Token);
            });

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
                        await Executor.ExecuteByTenantsAsync(() => HandleTimeEvents());
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(Scheduler).FullName, nameof(TimingLoop), e.GetType().Name, e.Message);
                    }
                }

                await Task.Delay(1000);
            }
        }

        private async Task HandleTimeEvents()
        {
            using var scope = ServiceProvider.CreateScope();

            try
            {
                var runner = scope.ServiceProvider.GetRequiredService<ScheduleExecutor>();

                await runner.ExecuteAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(Scheduler).FullName, nameof(HandleTimeEvents), e.GetType().Name, e.Message);
            }
        }

        private async Task InitiateBehaviors()
        {
            var tokens = BehaviorClassesInitializations.Instance.DefaultInstanceInitializationTokens;

            await Task.WhenAll(
                tokens.Select(async token =>
                {
                    token.RefreshEnvironment();

                    if (Locator.TryLocateBehavior(new BehaviorId(token.BehaviorClass, string.Empty), out var behavior))
                    {
                        await behavior.SendAsync(await token.InitializationRequestFactory(ServiceProvider, token.BehaviorClass));
                    }
                })
            );
        }

        private async Task HandleStartupEvents()
        {
            using var scope = ServiceProvider.CreateScope();

            try
            {
                var runner = scope.ServiceProvider.GetRequiredService<StartupExecutor>();

                await runner.ExecuteAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(Scheduler).FullName, nameof(HandleStartupEvents), e.GetType().Name, e.Message);
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