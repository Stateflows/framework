using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

public interface IStateMachineEndpointContext : IBehaviorEndpointContext
{
    [Obsolete("StateMachine context property is obsolete, use Behavior or CurrentState properties instead.")]
    IStateMachineContext StateMachine { get; }
    
    IBehaviorContext IBehaviorEndpointContext.Behavior => StateMachine;
        
    /// <summary>
    /// Information about current state of a State Machine
    /// </summary>
    IReadOnlyTree<IStateContext> CurrentStates => StateMachine.CurrentStates;
    
    public new static ValueTask<IStateMachineEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IStateMachineEndpointContext?>(metadata is { BehaviorClass.Type: BehaviorType.StateMachine }
            ? metadata.StateMachineContext.Value = new StateMachineEndpointContext()
            : null);
    }
}