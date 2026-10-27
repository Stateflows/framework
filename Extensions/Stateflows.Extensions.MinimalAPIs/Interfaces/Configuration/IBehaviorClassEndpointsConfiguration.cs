using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.Extensions.MinimalAPIs.Interfaces.Configuration;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Configures generated and custom endpoints for one behavior class.</summary>
public interface IBehaviorClassEndpointsConfiguration : IEventsConfiguration<IBehaviorClassEndpointsConfiguration>
{
    /// <summary>
    /// Disables endpoints mapping for Behavior Class
    /// </summary>
    void Disable();

    /// <summary>
    /// Configures all endpoints for Behavior Class
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <returns>This behavior-class configuration.</returns>
    IBehaviorClassEndpointsConfiguration ConfigureAllEndpoints(Action<IEndpointConfiguration> configureEndpointAction);

    /// <summary>
    /// Configures endpoint that returns Behavior instances for Behavior Class (f.e. GET "/stateMachines/Doc" by default)
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    /// <returns>This behavior-class configuration.</returns>
    IBehaviorClassEndpointsConfiguration ConfigureGetInstancesEndpoint(Action<IEndpointConfiguration> configureEndpointAction);

    /// <summary>
    /// Configures all custom endpoints of this behavior class.
    /// </summary>
    /// <param name="configureEndpointAction">Callback applying mapping rules and endpoint conventions.</param>
    /// <returns>This behavior-class configuration.</returns>
    IBehaviorClassEndpointsConfiguration ConfigureCustomEndpoints(Action<IEndpointConfiguration> configureEndpointAction);

    /// <summary>
    /// Configures endpoints related to default instance of Behavior Class
    /// </summary>
    /// <param name="configureInstanceAction">Callback configuring routes without an explicit instance segment.</param>
    /// <returns>This behavior-class configuration.</returns>
    IBehaviorClassEndpointsConfiguration ConfigureDefaultInstance(Action<IBehaviorInstanceEndpointsConfiguration> configureInstanceAction);

    /// <summary>
    /// Configures endpoints related to concrete instances of Behavior Class
    /// </summary>
    /// <param name="configureInstanceAction">Callback configuring routes with an explicit instance segment.</param>
    /// <returns>This behavior-class configuration.</returns>
    IBehaviorClassEndpointsConfiguration ConfigureStandardInstances(Action<IBehaviorInstanceEndpointsConfiguration> configureInstanceAction);
}