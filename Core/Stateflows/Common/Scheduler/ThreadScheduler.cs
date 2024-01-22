using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Stateflows.Common.Scheduler
{
    internal class ThreadScheduler : IHostedService
    {
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private readonly IStateflowsStorage Storage;

        private readonly IStateflowsLock Lock;

        private readonly IStateflowsTenantsManager TenantsManager;

        private readonly IBehaviorClassesProvider BehaviorClassesProvider;

        private readonly IBehaviorLocator Locator;

        private readonly IServiceScope Scope;

        private readonly ILogger<ThreadScheduler> Logger;

        private readonly TimeSpan LockTimeout = new TimeSpan(0, 0, 10);

        private BehaviorId HandlingLockId;

        public ThreadScheduler(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();

            Storage = Scope.ServiceProvider.GetRequiredService<IStateflowsStorage>();
            Lock = Scope.ServiceProvider.GetRequiredService<IStateflowsLock>();
            TenantsManager = Scope.ServiceProvider.GetRequiredService<IStateflowsTenantsManager>();
            BehaviorClassesProvider = Scope.ServiceProvider.GetRequiredService<IBehaviorClassesProvider>();
            Locator = Scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            Logger = Scope.ServiceProvider.GetRequiredService<ILogger<ThreadScheduler>>();
            HandlingLockId = new BehaviorId(nameof(ThreadScheduler), nameof(HandlingLockId), new BehaviorClass("", "").Environment);
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

                    _ = TenantsManager.ExecuteByTenantsAsync(_ => Task.Run(() => HandleTokens()));
                }

                await Task.Delay(1000);
            }
        }

        private async Task HandleTokens()
        {
            try
            {
                await using (await Lock.AquireLockAsync(HandlingLockId, LockTimeout))
                {
                    var contexts = await Storage.GetContextsToTimeTrigger(BehaviorClassesProvider.LocalBehaviorClasses);

                    await Task.WhenAll(
                        contexts.Select(context =>
                        {
                            if (Locator.TryLocateBehavior(context.Id, out var behavior))
                            {
                                var timeEvents = context.PendingTimeEvents.Values.Where(timeEvent => timeEvent.TriggerTime < DateTime.Now).ToArray();
                                _ = Task.WhenAll(timeEvents.Select(timeEvent => behavior.SendAsync(timeEvent)));
                            }

                            return Task.CompletedTask;
                        })
                    );
                }
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(ThreadScheduler).FullName, nameof(HandleTokens), e.GetType().Name, e.Message);
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