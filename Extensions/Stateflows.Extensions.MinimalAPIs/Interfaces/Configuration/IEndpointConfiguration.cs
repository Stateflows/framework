using Microsoft.AspNetCore.Builder;
using Stateflows.Common;

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
    
    /// <summary>
    /// Updates endpoint route
    /// </summary>
    IEndpointConfiguration AddMetadataBuilder<TMetadataBuilder>(Func<IServiceProvider, TMetadataBuilder>? builderFactory = null)
        where TMetadataBuilder : class, IEndpointMetadataBuilder;
}