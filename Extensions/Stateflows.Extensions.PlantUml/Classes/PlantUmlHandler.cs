using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.StateMachines;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal class PlantUmlHandler : IStateMachineEventHandler, IActivityEventHandler
    {
        private readonly IServiceProvider ServiceProvider;
        public PlantUmlHandler(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
        
        public Type EventType => typeof(PlantUmlInfoRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(StateMachines.Context.Interfaces.IEventContext<TEvent> context)
        {
            var inspection = ServiceProvider.GetRequiredService<IStateMachineInspection>();
            return Task.FromResult(HandleEvent(context.Event, () => inspection.GetPlantUml()));
        }

        public Task<EventStatus> TryHandleEventAsync<TEvent>(Activities.Context.Interfaces.IEventContext<TEvent> context)
        {
            var inspection = ServiceProvider.GetRequiredService<IActivityInspection>();
            return Task.FromResult(HandleEvent(context.Event, () => inspection.GetPlantUml()));
        }

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
