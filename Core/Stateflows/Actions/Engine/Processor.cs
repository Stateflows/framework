using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Registration;

namespace Stateflows.Actions.Engine
{
    internal class Processor : IEventProcessor
    {
        string IEventProcessor.BehaviorType => BehaviorType.Action;

        private readonly ActionsRegister Register;
        private readonly IStateflowsLock StateflowsLock;
        private readonly IServiceProvider ServiceProvider;

        public Processor(
            ActionsRegister register,
            IStateflowsLock stateflowsLock,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            StateflowsLock = stateflowsLock;
            ServiceProvider = serviceProvider;
        }

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions)
        {
            var result = EventStatus.Undelivered;

            var serviceProvider = ServiceProvider.CreateScope().ServiceProvider;

            var storage = serviceProvider.GetRequiredService<IStateflowsStorage>();

            var stateflowsContext = await storage.HydrateAsync(id);

            var key = stateflowsContext.Version != 0
                ? $"{id.Name}.{stateflowsContext.Version}"
                : $"{id.Name}.current";

            if (!Register.Actions.TryGetValue(key, out var action))
            {
                return result;
            }

            try
            {
                await using var lockHandle = await (
                    action.Reentrant
                        ? StateflowsLock.AquireNoLockAsync(id)
                        : StateflowsLock.AquireLockAsync(id)
                );

                if (action.Reentrant)
                {
                    stateflowsContext = await storage.HydrateAsync(id);
                }
                
                if (eventHolder is EventHolder<TokensInput> tokensInputHolder)
                {
                    await action.Delegate.Invoke(null);
                }
            }
            finally
            {
                if (stateflowsContext.Status == BehaviorStatus.Initialized)
                {
                    stateflowsContext.Version = action.Version;
                }

                stateflowsContext.Status = BehaviorStatus.Initialized;

                stateflowsContext.LastExecutedAt = DateTime.Now;

                // exceptions.AddRange(context.Exceptions);

                await storage.DehydrateAsync(stateflowsContext);
            }

            return result;
        }
    }
}
