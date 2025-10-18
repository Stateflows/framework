using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class FinalizationHandler(IStateflowsValueStorage valueStorage) : IStateMachineEventHandler
    {
        private IStateflowsValueStorage ValueStorage => valueStorage;
        
        public Type EventType => typeof(Finalize);

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            var executor = context.Behavior.GetExecutor();
            if (context.Event is Finalize request && !executor.Context.Finalized)
            {
                var finalized = await executor.ExitAsync();
                
                await ValueStorage.RemoveAsync(context.Behavior.Id, CommonValues.ForceFinalizeKey);

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
