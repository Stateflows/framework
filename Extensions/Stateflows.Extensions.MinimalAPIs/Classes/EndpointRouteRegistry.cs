using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Routing;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>
/// Detects event endpoints whose shortened route segments collide although their full event names differ.
/// Collisions involving a structural event fail fast; CLR-only collisions keep their previous behavior.
/// </summary>
internal static class EndpointRouteRegistry
{
    private static readonly ConditionalWeakTable<IEndpointRouteBuilder, Dictionary<string, (string EventName, bool IsStructural)>> MappedRoutes = new();

    public static void EnsureUnique(IEndpointRouteBuilder routeBuilder, string method, string route, string eventName, bool isStructural)
    {
        var routes = MappedRoutes.GetOrCreateValue(routeBuilder);
        var key = $"{method.ToUpperInvariant()} {route.ToLowerInvariant()}";
        lock (routes)
        {
            if (routes.TryGetValue(key, out var existing) && existing.EventName != eventName)
            {
                if (existing.IsStructural || isStructural)
                {
                    throw new InvalidOperationException(
                        $"Events '{existing.EventName}' and '{eventName}' map to the same endpoint '{key}'. " +
                        "Use ConfigureEventEndpoint(...) with UpdateRoute(...) to give one of them a distinct route.");
                }

                return;
            }

            routes.TryAdd(key, (eventName, isStructural));
        }
    }
}
