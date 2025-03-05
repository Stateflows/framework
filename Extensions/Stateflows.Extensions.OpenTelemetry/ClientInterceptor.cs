using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Extensions.OpenTelemetry.Headers;

namespace Stateflows.Extensions.OpenTelemetry
{
    public class ClientInterceptor : IStateflowsClientInterceptor
    {
        public async Task<bool> BeforeDispatchEventAsync(EventHolder eventHolder)
        {
            if (Activity.Current != null)
            {
                eventHolder.Headers.Add(new ActivityHeader() { Activity = Activity.Current });
            }

            return true;
        }

        public async Task AfterDispatchEventAsync(EventHolder eventHolder)
        { }
    }
}