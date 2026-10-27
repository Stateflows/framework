using System.Threading.Tasks;
using System;
using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal class PlantUmlActivityInterceptor : ActivityInterceptor
    {
        private readonly IActivityInspection Inspection;

        public PlantUmlActivityInterceptor(IActivityInspection inspection)
        {
            Inspection = inspection;
        }

        public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            var status = EventStatus.Failed;
            try
            {
                return status = await next();
            }
            finally
            {
                After(context, status);
            }

            void After(IEventContext<TEvent> context, EventStatus eventStatus)
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
}
