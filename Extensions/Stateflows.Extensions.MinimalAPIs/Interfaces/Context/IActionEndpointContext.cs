using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Actions;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

public interface IActionEndpointContext : IBehaviorEndpointContext
{
    [Obsolete("Action context property is obsolete, use Behavior property instead.")]
    IActionContext Action { get; }
    
    IBehaviorContext IBehaviorEndpointContext.Behavior => Action;
    
    public new static ValueTask<IActionEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IActionEndpointContext?>(metadata is { BehaviorClass.Type: BehaviorType.Action }
            ? metadata.ActionContext.Value = new ActionEndpointContext()
            : null);
    }
}