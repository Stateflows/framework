using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Scheduler
{
    internal class ScheduleExecutor(
        IServiceProvider services,
        IStateflowsLock @lock,
        IBehaviorClassesProvider behaviorClassesProvider,
        IBehaviorLocator behaviorLocator,
        ILogger<Scheduler> logger
    )
    {
        private readonly TimeSpan LockTimeout = new(0, 0, 10);

        private BehaviorId HandlingLockId = new(nameof(Scheduler), nameof(HandlingLockId), string.Empty);

        private readonly MethodInfo SendAsyncMethod = typeof(IBehavior).GetMethod(nameof(IBehavior.SendAsync));

        public async Task ExecuteAsync()
        {
            try
            {
                var storage = services.GetRequiredService<IStateflowsStorage>();

                await using (await @lock.AquireLockAsync(HandlingLockId, LockTimeout))
                {
                    var contexts = await storage.GetTimeTriggeredContextsAsync(behaviorClassesProvider.LocalBehaviorClasses);

                    foreach (var context in contexts)
                    {
                        var timeEvents = context.PendingTimeEvents.Values.Where(timeEvent => timeEvent.TriggerTime < DateTime.Now).ToArray();

                        if (timeEvents.Length != 0 && behaviorLocator.TryLocateBehavior(context.Id, out var behavior))
                        {
                            foreach (var timeEvent in timeEvents)
                            {
                                _ = SendAsyncMethod
                                    .MakeGenericMethod(timeEvent.GetType())
                                    .Invoke(behavior, [timeEvent, null]);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(ScheduleExecutor).FullName, nameof(ExecuteAsync), e.GetType().Name, e.Message);
            }
        }
    }
}