using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal class PlantUmlHandler : IStateMachineEventHandler//, IActivityEventHandler
    {
        public Type EventType => typeof(PlantUmlRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(Stateflows.StateMachines.Inspection.Interfaces.IEventInspectionContext<TEvent> context)
            where TEvent : Event
            => Task.FromResult(HandleEvent(context.Event, () => context.StateMachine.Inspection.GetPlantUml()));

        //public Task<EventStatus> TryHandleEventAsync<TEvent>(Stateflows.Activities.Inspection.Interfaces.IEventInspectionContext<TEvent> context)
        //    where TEvent : Event
        //    => Task.FromResult(HandleEvent(context.Event, () => context.Activity.Inspection.GetPlantUml()));

        private EventStatus HandleEvent<TEvent>(TEvent @event, Func<string> plantUmlGenerator)
            where TEvent : Event
        {
            if (@event is PlantUmlRequest)
            {
                var plantUml = plantUmlGenerator();
                (@event as PlantUmlRequest).Respond(new PlantUmlResponse()
                {
                    PlantUml = plantUml,
                    PlantUmlUrl = "http://www.plantuml.com/plantuml/png/" + PlantUmlTextEncoder.Encode(plantUml)
                }
                );

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
