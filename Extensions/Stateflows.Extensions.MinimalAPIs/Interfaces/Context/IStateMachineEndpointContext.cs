using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

public interface IStateMachineEndpointContext : IBehaviorEndpointContext
{
    /// <summary>
    /// Information about current state of a State Machine
    /// </summary>
    IReadOnlyTree<IStateContext> CurrentStates { get; }
    
    public new static ValueTask<IStateMachineEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IStateMachineEndpointContext?>(metadata is { BehaviorClass.Type: BehaviorType.StateMachine }
            ? metadata.StateMachineContext.Value = new StateMachineEndpointContext()
            : null);
    }
}