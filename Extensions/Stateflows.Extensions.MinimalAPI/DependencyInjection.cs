using Microsoft.AspNetCore.Routing;

namespace Stateflows
{
    public static class DependencyInjection
    {
        internal readonly static Dictionary<string, Action<IEndpointRouteBuilder>> Endpoints = new Dictionary<string, Action<IEndpointRouteBuilder>>();

        public static IEndpointRouteBuilder MapStateflowsEndpoints(this IEndpointRouteBuilder applicationBuilder)
        {
            foreach (var endpoint in Endpoints.Values)
            {
                endpoint(applicationBuilder);
            }

            return applicationBuilder;
        }
    }
}