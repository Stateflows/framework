using Microsoft.AspNetCore.Builder;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

public interface IEndpointConfiguration
{
    /// <summary>
    /// Disables endpoint mapping
    /// </summary>
    void Disable();
    
    /// <summary>
    /// Updates endpoint route
    /// </summary>
    /// <param name="routeUpdater">Action that returns updated route</param>
    IEndpointConfiguration UpdateRoute(Func<string, string> routeUpdater);
    
    /// <summary>
    /// Configures endpoint handler
    /// </summary>
    /// <param name="routeHandlerBuilderAction">Action that configures endpoint handler</param>
    IEndpointConfiguration ConfigureHandler(Action<IEndpointConventionBuilder> routeHandlerBuilderAction);
}