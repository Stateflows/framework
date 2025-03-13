using System;
using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal class PlantUmlStateMachineInterceptor : StateMachineInterceptor
    {
        private readonly IStateMachineInspection Inspection;

        public PlantUmlStateMachineInterceptor(IStateMachineInspection inspection)
        {
            Inspection = inspection;
        }

        public override void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        {
            try
            {
                if (context is IEventInspectionContext<TEvent> inspectionContext)
                {
                    if (Inspection.StateHasChanged)
                    {
                        context.Behavior.Publish(
                            new PlantUmlInfo()
                            {
                                PlantUml = Inspection.GetPlantUml()
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
        }
    }
}
