using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Actions;

/// <summary>Supplies class-specific HTTP mapping rules from a typed Action definition.</summary>
public interface IActionEndpointsConfiguration : IAction
{
    /// <summary>Configures the Action's generated and custom endpoints during API mapping.</summary>
    /// <param name="configuration">Configuration scoped to this Action class.</param>
    static abstract void ConfigureEndpoints(IBehaviorClassEndpointsConfiguration configuration);
}