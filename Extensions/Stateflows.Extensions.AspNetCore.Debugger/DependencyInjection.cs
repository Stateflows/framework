using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Stateflows.Transport.AspNetCore.SignalR;

namespace Stateflows
{
    public static class DependencyInjection
    {
        public static IEndpointRouteBuilder UseStateflowsDebugger(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHub<StateflowsHub>("/" + nameof(Stateflows));

            return endpoints;
        }
    }
}
