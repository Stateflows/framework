using Stateflows.Extensions.MinimalAPIs.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Describes an active state in an HTTP-visible State Machine configuration.</summary>
public class StateContext : IStateContext
{
    /// <inheritdoc/>
    public string Name { get; internal set; }
}
