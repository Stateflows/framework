using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    internal class TimeService : ITimeService, IHostedService
    {
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private readonly IStateflowsStorage Storage;

        private readonly IStateflowsLock Lock;

        private readonly CommonInterceptor Interceptor;

        private readonly IBehaviorClassesProvider BehaviorClassesProvider;

        private readonly IBehaviorLocator Locator;

        private readonly IServiceScope Scope;

        private BehaviorId LockId = new BehaviorId(nameof(Lock), nameof(TimeService), nameof(TimeService));

        public TimeService(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();

            Storage = Scope.ServiceProvider.GetRequiredService<IStateflowsStorage>();
            Lock = Scope.ServiceProvider.GetRequiredService<IStateflowsLock>();
            Interceptor = Scope.ServiceProvider.GetRequiredService<CommonInterceptor>();
            BehaviorClassesProvider = Scope.ServiceProvider.GetRequiredService<IBehaviorClassesProvider>();
            Locator = Scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
        }

        public async Task Clear(BehaviorId behaviorId, IEnumerable<string> ids)
        {
            if (!ids.Any())
            {
                return;
            }

            try
            {
                await Lock.LockAsync(LockId);
                await Storage.ClearTimeTokens(behaviorId, ids);
            }
            finally
            {
                await Lock.UnlockAsync(LockId);
            }
        }

        public async Task Register(TimeToken[] timeTokens)
        {
            if (!timeTokens.Any())
            {
                return;
            }

            try
            {
                await Lock.LockAsync(LockId);
                await Storage.AddTimeTokens(timeTokens);
            }
            finally
            {
                await Lock.UnlockAsync(LockId);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() => TimingLoop(CancellationTokenSource.Token));

            return Task.CompletedTask;
        }

        private async Task TimingLoop(CancellationToken cancellationToken)
        {
            var counter = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                counter++;

                if (counter > 60)
                {
                    _ = Task.Run(HandleTokens);

                    counter = 0;
                }

                await Task.Delay(1000);
            }
        }

        private async Task HandleTokens()
        {
            if (Interceptor.BeforeExecute())
            {
                try
                {
                    await Lock.LockAsync(LockId);

                    var tokens = await Storage.GetTimeTokens(BehaviorClassesProvider.LocalBehaviorClasses);

                    var passedTokens = tokens.Where(t => t.Event.ShouldTrigger(t.CreatedAt)).ToArray();

                    foreach (var token in passedTokens)
                    {
                        if (Locator.TryLocateBehavior(token.TargetId, out var behavior))
                        {
                            _ = behavior.SendAsync(token.Event);

                            await Storage.ClearTimeTokens(token.TargetId, new string[] { token.Id.ToString() });
                        }
                    }

                }
                finally
                {
                    await Lock.UnlockAsync(LockId);

                    Interceptor.AfterExecute();
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
