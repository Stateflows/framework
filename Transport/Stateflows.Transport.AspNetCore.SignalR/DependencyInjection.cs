using System.Diagnostics;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections;
using Stateflows.Transport.AspNetCore.SignalR;

namespace Stateflows
{
    public static class DependencyInjection
    {
        private static bool hubMapped = false;

        [DebuggerHidden]
        public static IEndpointRouteBuilder MapStateflowsTransportHub(this IEndpointRouteBuilder builder, Action<HttpConnectionDispatcherOptions>? configureOptions = null)
        {
            if (!hubMapped)
            {
                builder.MapHub<StateflowsHub>("/stateflows_v1", configureOptions);
                hubMapped = true;
            }
            
            return builder;
        }
    }
}
