using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.Extensions.PlantUml.Events;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal class PlantUmlHandler : IStateMachineEventHandler
    {
        public string EventName => EventInfo<PlantUmlRequest>.Name;

        public Task<bool> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
        {
            if (context.Event is PlantUmlRequest)
            {
                var plantUml = context.StateMachine.Inspection.GetPlantUml();
                (context.Event as PlantUmlRequest).Respond(new PlantUmlResponse()
                    {
                        PlantUml = plantUml,
                        PlantUmlUrl = "http://www.plantuml.com/plantuml/png/" + PlantUmlTextEncoder.Encode(plantUml)
                    }
                );

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
