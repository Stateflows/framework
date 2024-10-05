using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal class StateMachineInterceptor : IStateMachineInterceptor
    {
        public Task AfterHydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
        {
            if (
                context is IEventInspectionContext<TEvent> inspectionContext &&
                inspectionContext.StateMachine.Inspection.StateHasChanged
            )
            {
                context.StateMachine.Publish(
                    new Events.PlantUmlInfo()
                    {
                        PlantUml = inspectionContext.StateMachine.Inspection.GetPlantUml()
                    }
                );
            }

            return Task.CompletedTask;
        }

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
            => Task.FromResult(true);
    }
}
