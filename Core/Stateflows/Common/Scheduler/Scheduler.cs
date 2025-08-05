using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Initializer;

namespace Stateflows.Common.Scheduler
{
    internal class Scheduler : IHostedService
    {
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        // private readonly IStateflowsInitializer Initializer;
        private readonly IStateflowsTenantExecutor Executor;
        private readonly IServiceScope Scope;
        private readonly ILogger<Scheduler> Logger;
        private IBehaviorLocator Locator;
        private readonly IHostApplicationLifetime Lifetime;

        private IServiceProvider ServiceProvider
            => Scope.ServiceProvider;

        private readonly MethodInfo SendAsyncMethod = typeof(IBehavior).GetMethod(nameof(IBehavior.SendAsync));

        public Scheduler(IServiceProvider serviceProvider, /*IStateflowsInitializer initializer,*/ IHostApplicationLifetime lifetime)
        {
            Scope = serviceProvider.CreateScope();
            // Initializer = initializer;
            Locator = ServiceProvider.GetRequiredService<IBehaviorLocator>();
            Executor = ServiceProvider.GetRequiredService<IStateflowsTenantExecutor>();
            Logger = ServiceProvider.GetRequiredService<ILogger<Scheduler>>();
            Lifetime = lifetime;
        }

        public void ApplicationStarted()
        {
            Task.WaitAll(Executor.ExecuteByTenantsAsync(() => InitiateBehaviors()));
            Task.WaitAll(Executor.ExecuteByTenantsAsync(() => HandleStartupEvents()));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Initializer.Initialize(ServiceProvider);
            
            Lifetime.ApplicationStarted.Register(ApplicationStarted);

            _ = Task.Run(async () =>
            {
                await TimingLoop(CancellationTokenSource.Token).ConfigureAwait(false);
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
                        await Executor.ExecuteByTenantsAsync(() => HandleTimeEvents()).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(Scheduler).FullName, nameof(TimingLoop), e.GetType().Name, e.Message);
                    }
                }

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }

        private async Task HandleTimeEvents()
        {
            using var scope = ServiceProvider.CreateScope();

            try
            {
                var runner = scope.ServiceProvider.GetRequiredService<ScheduleExecutor>();

                await runner.ExecuteAsync().ConfigureAwait(false);
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
                        var initializationEvent = await token.InitializationRequestFactory(ServiceProvider, token.BehaviorClass).ConfigureAwait(false);
                        await (SendAsyncMethod
                            .MakeGenericMethod(initializationEvent.GetType())
                            .Invoke(behavior, new object[] { initializationEvent, null }) as Task);
                    }
                })
            ).ConfigureAwait(false);
        }

        private async Task HandleStartupEvents()
        {
            using var scope = ServiceProvider.CreateScope();

            try
            {
                var runner = scope.ServiceProvider.GetRequiredService<StartupExecutor>();

                await runner.ExecuteAsync().ConfigureAwait(false);
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