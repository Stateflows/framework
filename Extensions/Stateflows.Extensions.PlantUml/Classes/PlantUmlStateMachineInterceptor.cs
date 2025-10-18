using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines;
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

        public override void AfterProcessEvent<TEvent>(StateMachines.Context.Interfaces.IEventContext<TEvent> context, EventStatus eventStatus)
        {
            try
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
