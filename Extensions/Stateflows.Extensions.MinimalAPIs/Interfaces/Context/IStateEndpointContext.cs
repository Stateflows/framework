using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

/// <summary>Provides access to the state owning a state-scoped custom HTTP endpoint.</summary>
public interface IStateEndpointContext : IStateMachineEndpointContext
{
    /// <summary>Gets the active state to which the endpoint is scoped.</summary>
    IStateContext State { get; }
    
    /// <summary>Binds a state-scoped endpoint context from the current route metadata.</summary>
    /// <param name="context">Current HTTP request.</param>
    /// <param name="parameter">Handler parameter being bound.</param>
    /// <returns>An endpoint context, or null if the route does not target a State Machine.</returns>
    public new static ValueTask<IStateEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IStateEndpointContext?>(metadata is { BehaviorClass.Type: BehaviorType.StateMachine }
            ? metadata.StateContext.Value = new StateEndpointContext()
            : null);
    }
}