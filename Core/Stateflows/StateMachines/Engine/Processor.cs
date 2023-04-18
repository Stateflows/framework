using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Engine
{
    internal class Processor : IEventProcessor
    {
        public string BehaviorType => nameof(StateMachine);

        public StateMachinesRegister Register { get; }
        public IStateflowsStorage Storage { get; }
        public Dictionary<string, IStateMachineEventHandler> EventHandlers { get; }
        public IServiceProvider ServiceProvider { get; }

        public Processor(
            StateMachinesRegister register, 
            IStateflowsStorage storage,
            IEnumerable<IStateMachineEventHandler> eventHandlers,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            Storage = storage;
            ServiceProvider = serviceProvider;
            EventHandlers = eventHandlers.ToDictionary(h => h.EventName, h => h);
        }

        private async Task<bool> TryHandleEventAsync<TEvent>(EventContext<TEvent> context)
            where TEvent : Event, new()
            => (EventHandlers.TryGetValue(context.Event.Name, out var eventHandler) && await eventHandler.TryHandleEventAsync(context));

        public async Task<bool> ProcessEventAsync<TEvent>(BehaviorId id, TEvent @event)
            where TEvent : Event, new()
        {
            var result = false;

            if (!Register.StateMachines.TryGetValue(id.Name, out var graph))
            {
                return result;
            }

            using (var executor = new Executor(Register, graph, ServiceProvider))
            {
                var context = new RootContext(await Storage.Hydrate(id));

                context.Context.Values[Constants.Event] = @event;

                if (await executor.Hydrate(context))
                {
                    result = await TryHandleEventAsync(new EventContext<TEvent>(context))
                        ? true
                        : await executor.ProcessAsync(@event);

                    await Storage.Dehydrate((await executor.Dehydrate()).Context);
                }
            }

            return result;
        }
    }
}
