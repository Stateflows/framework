using System.Diagnostics;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Stateflows.Transport.AspNetCore.SignalR;

namespace Stateflows
{
    public static class DependencyInjection
    {
        [DebuggerHidden]
        public static IEndpointRouteBuilder MapStateflowsTransportHub(this IEndpointRouteBuilder builder, Action<HttpConnectionDispatcherOptions>? configureOptions = null)
        {
            builder.MapHub<StateflowsHub>("/stateflows_v1", configureOptions);
            return builder;
        }
    }
}
