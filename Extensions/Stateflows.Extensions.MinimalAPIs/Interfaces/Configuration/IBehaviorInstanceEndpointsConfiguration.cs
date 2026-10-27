namespace Stateflows.Extensions.MinimalAPIs.Interfaces.Configuration;

/// <summary>Configures either default-instance routes or routes with an explicit instance segment.</summary>
public interface IBehaviorInstanceEndpointsConfiguration : IEventsConfiguration<IBehaviorClassEndpointsConfiguration>
{
    /// <summary>Disables endpoints for the selected instance-route category.</summary>
    void Disable();
    /// <summary>Applies a rule to all endpoints for the selected instance-route category.</summary>
    /// <param name="configureEndpointAction">Callback configuring endpoint mapping and conventions.</param>
    /// <returns>This instance-route configuration.</returns>
    IBehaviorInstanceEndpointsConfiguration ConfigureAllEndpoints(Action<IEndpointConfiguration> configureEndpointAction);
    /// <summary>Applies a rule to custom endpoints for the selected instance-route category.</summary>
    /// <param name="configureEndpointAction">Callback configuring endpoint mapping and conventions.</param>
    /// <returns>This instance-route configuration.</returns>
    IBehaviorInstanceEndpointsConfiguration ConfigureCustomEndpoints(Action<IEndpointConfiguration> configureEndpointAction);
}
