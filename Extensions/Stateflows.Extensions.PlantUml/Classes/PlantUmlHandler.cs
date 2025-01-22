using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal class PlantUmlHandler : IStateMachineEventHandler, IActivityEventHandler
    {
        public Type EventType => typeof(PlantUmlInfoRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(StateMachines.Inspection.Interfaces.IEventInspectionContext<TEvent> context)

            => Task.FromResult(HandleEvent(context.Event, () => context.StateMachine.Inspection.GetPlantUml()));

        public Task<EventStatus> TryHandleEventAsync<TEvent>(Activities.Inspection.Interfaces.IEventInspectionContext<TEvent> context)

            => Task.FromResult(HandleEvent(context.Event, () => context.Activity.Inspection.GetPlantUml()));

        private EventStatus HandleEvent<TEvent>(TEvent @event, Func<string> plantUmlGenerator)
        {
            if (@event is PlantUmlInfoRequest request)
            {
                var plantUml = plantUmlGenerator();
                request.Respond(new PlantUmlInfo() { PlantUml = plantUml });

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
