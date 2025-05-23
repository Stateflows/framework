using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Extensions.OpenTelemetry.Headers;

namespace Stateflows.Extensions.OpenTelemetry
{
    public class ClientInterceptor : IStateflowsClientInterceptor
    {
        public bool BeforeDispatchEvent(EventHolder eventHolder)
        {
            if (Activity.Current != null)
            {
                eventHolder.Headers.Add(new ActivityHeader() { Activity = Activity.Current });
            }

            return true;
        }

        public void AfterDispatchEvent(EventHolder eventHolder)
        { }
    }
}