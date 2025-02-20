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
            => HandleEventAsync(context.Event, async () => (await context.StateMachine.GetInspectionAsync()).GetPlantUml());

        public Task<EventStatus> TryHandleEventAsync<TEvent>(Activities.Inspection.Interfaces.IEventInspectionContext<TEvent> context)
            => HandleEventAsync(context.Event, async () => (await context.Activity.GetInspectionAsync()).GetPlantUml());

        private async Task<EventStatus> HandleEventAsync<TEvent>(TEvent @event, Func<Task<string>> plantUmlGenerator)
        {
            if (@event is PlantUmlInfoRequest request)
            {
                var plantUml = await plantUmlGenerator();
                request.Respond(new PlantUmlInfo() { PlantUml = plantUml });

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
