﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;

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

        private BehaviorId HandlingLockId;

        public ThreadScheduler(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();

            Storage = Scope.ServiceProvider.GetRequiredService<IStateflowsStorage>();
            Lock = Scope.ServiceProvider.GetRequiredService<IStateflowsLock>();
            TenantsManager = Scope.ServiceProvider.GetRequiredService<IStateflowsTenantsManager>();
            BehaviorClassesProvider = Scope.ServiceProvider.GetRequiredService<IBehaviorClassesProvider>();
            Locator = Scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
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

                    _ = Task.Run(() => TenantsManager.ExecuteByTenants(HandleTokens));
                }

                await Task.Delay(1000);
            }
        }

        private async Task HandleTokens(string tenantId)
        {
            await using (await Lock.AquireLockAsync(HandlingLockId))
            {
                var contexts = await Storage.GetContextsToTimeTrigger(BehaviorClassesProvider.LocalBehaviorClasses);

                await Task.WhenAll(
                    contexts.Select(context =>
                    {
                        var timeEvent = context.PendingTimeEvents.Values.FirstOrDefault(timeEvent => timeEvent.TriggerTime < DateTime.Now);
                        return (timeEvent != null && Locator.TryLocateBehavior(context.Id, out var behavior))
                            ? behavior.SendAsync(timeEvent)
                            : Task.CompletedTask;
                    })
                );
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