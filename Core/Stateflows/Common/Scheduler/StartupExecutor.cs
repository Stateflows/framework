using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Scheduler
{
    internal class StartupExecutor
    {
        private readonly IStateflowsLock Lock;

        private readonly IBehaviorClassesProvider BehaviorClassesProvider;

        private readonly IBehaviorLocator Locator;

        private readonly ILogger<Scheduler> Logger;

        private readonly TimeSpan LockTimeout = new TimeSpan(0, 0, 10);

        private BehaviorId HandlingLockId;

        private readonly IServiceProvider Services;

        public StartupExecutor(
            IServiceProvider services,
            IStateflowsLock @lock,
            IBehaviorClassesProvider behaviorClassesProvider,
            IBehaviorLocator behaviorLocator,
            ILogger<Scheduler> logger
        )
        {
            Services = services;
            Lock = @lock;
            BehaviorClassesProvider = behaviorClassesProvider;
            Locator = behaviorLocator;
            Logger = logger;
            HandlingLockId = new BehaviorId(nameof(Scheduler), nameof(HandlingLockId), string.Empty);
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var storage = Services.GetRequiredService<IStateflowsStorage>();

                await using (await Lock.AquireLockAsync(HandlingLockId, LockTimeout))
                {
                    var contexts = await storage.GetStartupTriggeredContextsAsync(BehaviorClassesProvider.LocalBehaviorClasses);

                    foreach (var context in contexts)
                    {
                        var startupEvents = context.PendingStartupEvents.Values.ToArray();

                        if (startupEvents.Any() && Locator.TryLocateBehavior(context.Id, out var behavior))
                        {
                            foreach (var startupEvent in startupEvents)
                            {
                                await behavior.SendAsync(startupEvent);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StartupExecutor).FullName, nameof(ExecuteAsync), e.GetType().Name, e.Message);
            }
        }
    }
}