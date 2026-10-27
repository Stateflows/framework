using Microsoft.AspNetCore.Routing;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Maps custom HTTP endpoints for a behavior class or an active state/node scope.</summary>
/// <remarks>
/// Patterns are appended to the behavior's instance route. When the class has a default instance,
/// a route without the instance segment is also mapped. The returned group configures both routes.
/// State-scoped and structured-node-scoped endpoints are filtered by the active configuration.
/// See <see href="https://github.com/Stateflows/framework/wiki/extension-minimal-apis"/>.
/// </remarks>
public interface IEndpointsBuilder
{
    /// <summary>Maps a custom GET endpoint.</summary>
    /// <param name="pattern">Route suffix, including any leading slash and route parameters.</param>
    /// <param name="handler">Minimal API handler; behavior endpoint context parameters are supplied by Stateflows.</param>
    /// <returns>The route group to which endpoint conventions, such as authorization, can be applied.</returns>
    RouteGroupBuilder AddGet(string pattern, Delegate handler);
    /// <summary>Maps a custom POST endpoint.</summary>
    /// <param name="pattern">Route suffix, including any leading slash and route parameters.</param>
    /// <param name="handler">Minimal API handler; behavior endpoint context parameters are supplied by Stateflows.</param>
    /// <returns>The route group containing the mapped endpoints.</returns>
    RouteGroupBuilder AddPost(string pattern, Delegate handler);
    /// <summary>Maps a custom PATCH endpoint.</summary>
    /// <param name="pattern">Route suffix, including any leading slash and route parameters.</param>
    /// <param name="handler">Minimal API handler; behavior endpoint context parameters are supplied by Stateflows.</param>
    /// <returns>The route group containing the mapped endpoints.</returns>
    RouteGroupBuilder AddPatch(string pattern, Delegate handler);
    /// <summary>Maps a custom PUT endpoint.</summary>
    /// <param name="pattern">Route suffix, including any leading slash and route parameters.</param>
    /// <param name="handler">Minimal API handler; behavior endpoint context parameters are supplied by Stateflows.</param>
    /// <returns>The route group containing the mapped endpoints.</returns>
    RouteGroupBuilder AddPut(string pattern, Delegate handler);
    /// <summary>Maps a custom DELETE endpoint.</summary>
    /// <param name="pattern">Route suffix, including any leading slash and route parameters.</param>
    /// <param name="handler">Minimal API handler; behavior endpoint context parameters are supplied by Stateflows.</param>
    /// <returns>The route group containing the mapped endpoints.</returns>
    RouteGroupBuilder AddDelete(string pattern, Delegate handler);
    /// <summary>Maps a custom endpoint for the specified HTTP methods.</summary>
    /// <param name="pattern">Route suffix, including any leading slash and route parameters.</param>
    /// <param name="methods">HTTP methods accepted by the endpoint.</param>
    /// <param name="handler">Minimal API handler; behavior endpoint context parameters are supplied by Stateflows.</param>
    /// <returns>The route group containing the mapped endpoints.</returns>
    RouteGroupBuilder AddMethods(string pattern, string[] methods, Delegate handler);
}