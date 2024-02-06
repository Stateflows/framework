﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Stateflows.Common.Scheduler
{
    internal class ScheduleExecutor
    {
        private readonly IStateflowsLock Lock;

        private readonly IBehaviorClassesProvider BehaviorClassesProvider;

        private readonly IBehaviorLocator Locator;

        private readonly ILogger<ThreadScheduler> Logger;

        private readonly TimeSpan LockTimeout = new TimeSpan(0, 0, 10);

        private BehaviorId HandlingLockId;

        private readonly IServiceProvider Services;

        public ScheduleExecutor(
            IServiceProvider services,
            IStateflowsLock @lock,
            IBehaviorClassesProvider behaviorClassesProvider,
            IBehaviorLocator behaviorLocator,
            ILogger<ThreadScheduler> logger
        )
        {
            Services = services;
            Lock = @lock;
            BehaviorClassesProvider = behaviorClassesProvider;
            Locator = behaviorLocator;
            Logger = logger;
            HandlingLockId = new BehaviorId(nameof(ThreadScheduler), nameof(HandlingLockId), new BehaviorClass("", "").Environment);
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var storage = Services.GetRequiredService<IStateflowsStorage>();

                await using (await Lock.AquireLockAsync(HandlingLockId, LockTimeout))
                {
                    var contexts = await storage.GetContextsToTimeTriggerAsync(BehaviorClassesProvider.LocalBehaviorClasses);

                    foreach (var context in contexts)
                    {
                        if (Locator.TryLocateBehavior(context.Id, out var behavior))
                        {
                            var timeEvents = context.PendingTimeEvents.Values.Where(timeEvent => timeEvent.TriggerTime < DateTime.Now).ToArray();
                            foreach (var timeEvent in timeEvents)
                            {
                                await behavior.SendAsync(timeEvent);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(ScheduleExecutor).FullName, nameof(ExecuteAsync), e.GetType().Name, e.Message);
            }
        }
    }
}