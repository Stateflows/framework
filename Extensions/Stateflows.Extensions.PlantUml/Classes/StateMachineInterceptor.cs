using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal class StateMachineInterceptor : IStateMachineInterceptor
    {
        public Task AfterHydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task AfterProcessEventAsync(IEventContext<Event> context)
        {
            if (
                context is IEventInspectionContext<Event> inspectionContext &&
                inspectionContext.StateMachine.Inspection.StateHasChanged
            )
            {
                context.StateMachine.Publish(
                    new PlantUmlNotification()
                    {
                        PlantUml = inspectionContext.StateMachine.Inspection.GetPlantUml()
                    }
                );
            }

            return Task.CompletedTask;
        }

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
            => Task.FromResult(true);
    }
}
