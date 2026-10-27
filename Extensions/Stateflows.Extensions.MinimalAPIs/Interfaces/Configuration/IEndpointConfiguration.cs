using Microsoft.AspNetCore.Builder;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

/// <summary>Configures the endpoints selected by the enclosing mapping rule.</summary>
public interface IEndpointConfiguration
{
    /// <summary>
    /// Disables the selected endpoints.
    /// </summary>
    void Disable();
    
    /// <summary>
    /// Transforms the selected endpoint routes during mapping.
    /// </summary>
    /// <param name="routeUpdater">Function receiving the current route and returning its replacement.</param>
    /// <returns>This configuration for further endpoint rules.</returns>
    IEndpointConfiguration UpdateRoute(Func<string, string> routeUpdater);
    
    /// <summary>
    /// Applies ASP.NET Core endpoint conventions to the selected endpoints.
    /// </summary>
    /// <param name="routeHandlerBuilderAction">Callback applying metadata, authorization, or other endpoint conventions.</param>
    /// <returns>This configuration for further endpoint rules.</returns>
    IEndpointConfiguration ConfigureHandler(Action<IEndpointConventionBuilder> routeHandlerBuilderAction);
}