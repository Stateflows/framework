namespace Stateflows.Extensions.MinimalAPIs.Interfaces.Configuration;

public interface IBehaviorInstanceEndpointsConfiguration : IEventsConfiguration<IBehaviorClassEndpointsConfiguration>
{
    void Disable();
    IBehaviorInstanceEndpointsConfiguration ConfigureAllEndpoints(Action<IEndpointConfiguration> configureEndpointAction);
    IBehaviorInstanceEndpointsConfiguration ConfigureCustomEndpoints(Action<IEndpointConfiguration> configureEndpointAction);
}

