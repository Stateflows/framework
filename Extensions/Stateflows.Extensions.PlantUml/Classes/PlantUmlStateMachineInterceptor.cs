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
    internal class PlantUmlStateMachineInterceptor : StateMachineInterceptor
    {
        // public override Task AfterHydrateAsync(IStateMachineActionContext context)
        //     => Task.CompletedTask;

        public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, Func<IEventActionContext<TEvent>, Task<EventStatus>> next)
        {
            var result = await next(context);
            
            try
            {
                if (context is IEventInspectionContext<TEvent> inspectionContext)
                {
                    var inspection = await inspectionContext.StateMachine.GetInspectionAsync();

                    if (inspection.StateHasChanged)
                    {
                        context.Behavior.Publish(
                            new PlantUmlInfo()
                            {
                                PlantUml = inspection.GetPlantUml()
                            }
                        );
                    }
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

            return result;
        }

        // public override async Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, EventStatus eventStatus)
        // {
        //     try
        //     {
        //         if (context is IEventInspectionContext<TEvent> inspectionContext)
        //         {
        //             var inspection = await inspectionContext.StateMachine.GetInspectionAsync();
        //
        //             if (inspection.StateHasChanged)
        //             {
        //                 context.Behavior.Publish(
        //                     new PlantUmlInfo()
        //                     {
        //                         PlantUml = inspection.GetPlantUml()
        //                     }
        //                 );
        //             }
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Trace.WriteLine($"⦗→s⦘ PlantUML exception '{e.GetType().Name}' thrown with message '{e.Message}'; stack trace:");
        //         foreach (var line in e.StackTrace.Split('\n'))
        //         {
        //             Trace.WriteLine($"⦗→s⦘     {line}");
        //         }
        //     }
        // }
        //
        // public override Task BeforeDehydrateAsync(IStateMachineActionContext context)
        //     => Task.CompletedTask;
        //
        // public override Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
        //     => Task.FromResult(true);
    }
}
