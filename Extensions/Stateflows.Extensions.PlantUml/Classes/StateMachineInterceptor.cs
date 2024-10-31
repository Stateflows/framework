using System;
using System.Diagnostics;
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

        public Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
        {
            try
            {
                if (context is IEventInspectionContext<TEvent> inspectionContext &&
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
            }
            catch (Exception e)
            {
                Trace.WriteLine($"⦗→s⦘ PlantUML exception '{e.GetType().Name}' thrown with message '{e.Message}'; stack trace:");
                foreach (var line in e.StackTrace.Split('\n'))
                {
                    Trace.WriteLine($"⦗→s⦘     {line}");
                }
            }

            return Task.CompletedTask;
        }

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
            => Task.FromResult(true);
    }
}
