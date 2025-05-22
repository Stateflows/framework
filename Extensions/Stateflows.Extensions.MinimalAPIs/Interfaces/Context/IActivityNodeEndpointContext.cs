using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

public interface IActivityNodeEndpointContext : IActivityEndpointContext
{
    IActivityNodeContext Node { get; }
    
    public new static ValueTask<IActivityNodeEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IActivityNodeEndpointContext?>(metadata is { BehaviorClass.Type: BehaviorType.Activity }
            ? metadata.ActivityNodeContext.Value = new ActivityNodeEndpointContext()
            : null);
    }
}