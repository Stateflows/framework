using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

public interface IBehaviorEndpointContext : IBehaviorLocator
{
    IBehaviorContext Behavior { get; }
    
    public static ValueTask<IBehaviorEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IBehaviorEndpointContext?>(metadata != null
            ? metadata.BehaviorContext.Value = new BehaviorEndpointContext()
            : null);
    }

    EndpointResponse Response();

    EndpointResponse<T> Response<T>(T result);
}