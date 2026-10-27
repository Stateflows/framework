using Stateflows.Extensions.MinimalAPIs.Interfaces;
using IStateContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IStateContext;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Implements a custom endpoint context scoped to an active State Machine state.</summary>
public class StateEndpointContext : StateMachineEndpointContext, IStateEndpointContext
{
    /// <inheritdoc/>
    public IStateContext State { get; internal set; }
}