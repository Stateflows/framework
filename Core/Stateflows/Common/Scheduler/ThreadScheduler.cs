using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Scheduler
{
    internal class ThreadScheduler : IStateflowsScheduler, IHostedService
    {
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private readonly IStateflowsStorage Storage;

        private readonly IStateflowsLock Lock;

        private readonly IStateflowsTenantsManager TenantsManager;

        private readonly IBehaviorClassesProvider BehaviorClassesProvider;

        private readonly IBehaviorLocator Locator;

        private readonly IServiceScope Scope;

        private BehaviorId StorageLockId;

        private BehaviorId HandlingLockId;

        public ThreadScheduler(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();

            Storage = Scope.ServiceProvider.GetRequiredService<IStateflowsStorage>();
            Lock = Scope.ServiceProvider.GetRequiredService<IStateflowsLock>();
            TenantsManager = Scope.ServiceProvider.GetRequiredService<IStateflowsTenantsManager>();
            BehaviorClassesProvider = Scope.ServiceProvider.GetRequiredService<IBehaviorClassesProvider>();
            Locator = Scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
            StorageLockId = new BehaviorId(nameof(ThreadScheduler), nameof(StorageLockId), new BehaviorClass("", "").Environment);
            HandlingLockId = new BehaviorId(nameof(ThreadScheduler), nameof(HandlingLockId), new BehaviorClass("", "").Environment);
        }

        public async Task Clear(BehaviorId behaviorId, IEnumerable<string> ids)
        {
            if (!ids.Any())
            {
                return;
            }

            await using (await Lock.AquireLockAsync(StorageLockId))
            {
                await Storage.ClearTimeTokens(behaviorId, ids);
            }
        }

        public async Task Register(TimeToken[] timeTokens)
        {
            if (!timeTokens.Any())
            {
                return;
            }

            await using (await Lock.AquireLockAsync(StorageLockId))
            {
                await Storage.AddTimeTokens(timeTokens);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() => TimingLoop(CancellationTokenSource.Token));

            return Task.CompletedTask;
        }

        private async Task TimingLoop(CancellationToken cancellationToken)
        {
            var lastTick = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

            while (!cancellationToken.IsCancellationRequested)
            {
                var diffInSeconds = (DateTime.Now - lastTick).TotalSeconds;

                if (diffInSeconds >= 60)
                {
                    lastTick = DateTime.Now;

                    _ = Task.Run(() => TenantsManager.ExecuteByTenants(HandleTokens));
                }

                await Task.Delay(1000);
            }
        }

        private async Task HandleTokens(string tenantId)
        {
            IEnumerable<TimeToken> tokens;

            await using (await Lock.AquireLockAsync(HandlingLockId))
            {
                await using (await Lock.AquireLockAsync(StorageLockId))
                {
                    tokens = await Storage.GetTimeTokens(BehaviorClassesProvider.LocalBehaviorClasses);
                }

                var passedTokens = tokens.Where(t => t.Event.ShouldTrigger(t.CreatedAt)).ToArray();

                await Task.WhenAll(
                    passedTokens.Select(async token =>
                    {
                        if (Locator.TryLocateBehavior(token.TargetId, out var behavior))
                        {
                            token.Event.EdgeIdentifier = token.EdgeIdentifier;

                            var result = await behavior.SendAsync(token.Event);

                            if (result.Status != EventStatus.Consumed)
                            {
                                await using (await Lock.AquireLockAsync(StorageLockId))
                                {
                                    await Storage.ClearTimeTokens(token.TargetId, new string[] { token.Id });
                                }
                            }
                        }
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