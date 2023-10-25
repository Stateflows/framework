using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal class PlantUmlHandler : IStateMachineEventHandler
    {
        public Type EventType => typeof(PlantUmlRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(Stateflows.StateMachines.Inspection.Interfaces.IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
            => Task.FromResult(HandleEvent(context.Event, () => context.StateMachine.Inspection.GetPlantUml()));

        private EventStatus HandleEvent<TEvent>(TEvent @event, Func<string> plantUmlGenerator)
            where TEvent : Event, new()
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
