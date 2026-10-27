using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

/// <summary>Provides the State Machine's active configuration and behavior operations in a custom HTTP endpoint.</summary>
public interface IStateMachineEndpointContext : IBehaviorEndpointContext
{
    /// <summary>
    /// Information about current state of a State Machine
    /// </summary>
    IReadOnlyTree<IStateContext> CurrentStates { get; }
    
    /// <summary>Binds a State Machine endpoint context from the current route metadata.</summary>
    /// <param name="context">Current HTTP request.</param>
    /// <param name="parameter">Handler parameter being bound.</param>
    /// <returns>An endpoint context, or null if the route does not target a State Machine.</returns>
    public new static ValueTask<IStateMachineEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IStateMachineEndpointContext?>(metadata is { BehaviorClass.Type: BehaviorType.StateMachine }
            ? metadata.StateMachineContext.Value = new StateMachineEndpointContext()
            : null);
    }
}