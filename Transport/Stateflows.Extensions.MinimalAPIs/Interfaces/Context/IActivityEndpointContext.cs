using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stateflows.Activities;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

public interface IActivityEndpointContext : IBehaviorEndpointContext
{
    [Obsolete("Activity context property is obsolete, use Behavior or ActiveNodes properties instead.")]
    IActivityContext Activity { get; }
    
    IBehaviorContext IBehaviorEndpointContext.Behavior => Activity;
        
    /// <summary>
    /// Information about current state of a State Machine
    /// </summary>
    IReadOnlyTree<IActivityNodeContext> ActiveNodes { get; }
    
    public new static ValueTask<IActivityEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IActivityEndpointContext?>(metadata is { BehaviorClass.Type: BehaviorType.Activity }
            ? metadata.ActivityContext.Value = new ActivityEndpointContext()
            : null);
    }
}