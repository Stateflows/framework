using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.Extensions.MinimalAPIs.Interfaces.Configuration;

namespace Stateflows.Extensions.MinimalAPIs;

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
    IBehaviorClassEndpointsConfiguration ConfigureAllEndpoints(Action<IEndpointConfiguration> configureEndpointAction);

    /// <summary>
    /// Configures endpoint that returns Behavior instances for Behavior Class (f.e. GET "/stateMachines/Doc" by default)
    /// </summary>
    /// <param name="configureEndpointAction">Configuration action</param>
    IBehaviorClassEndpointsConfiguration ConfigureGetInstancesEndpoint(Action<IEndpointConfiguration> configureEndpointAction);

    /// <summary>
    /// Configures custom endpoint of Behavior Class
    /// </summary>
    /// <param name="configureEndpointAction"></param>
    /// <returns></returns>
    IBehaviorClassEndpointsConfiguration ConfigureCustomEndpoints(Action<IEndpointConfiguration> configureEndpointAction);

    /// <summary>
    /// Configures endpoints related to default instance of Behavior Class
    /// </summary>
    /// <param name="configureInstanceAction"></param>
    /// <returns></returns>
    IBehaviorClassEndpointsConfiguration ConfigureDefaultInstance(Action<IBehaviorInstanceEndpointsConfiguration> configureInstanceAction);

    /// <summary>
    /// Configures endpoints related to concrete instances of Behavior Class
    /// </summary>
    /// <param name="configureInstanceAction"></param>
    /// <returns></returns>
    IBehaviorClassEndpointsConfiguration ConfigureStandardInstances(Action<IBehaviorInstanceEndpointsConfiguration> configureInstanceAction);
}