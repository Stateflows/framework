using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

public interface IStateEndpointContext : IStateMachineEndpointContext
{
    IStateContext State { get; }
    
    public new static ValueTask<IStateEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IStateEndpointContext?>(metadata is { BehaviorClass.Type: BehaviorType.StateMachine }
            ? metadata.StateContext.Value = new StateEndpointContext()
            : null);
    }
}