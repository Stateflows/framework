using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics;
using Stateflows.Transport.AspNetCore.SignalR;

namespace Stateflows
{
    public static class DependencyInjection
    {
        [DebuggerHidden]
        public static IEndpointRouteBuilder MapStateflowsTransportHub(this IEndpointRouteBuilder builder)
        {
            builder.MapHub<StateflowsHub>("/stateflows_v1");
            return builder;
        }
    }
}
