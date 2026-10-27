using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

/// <summary>Provides access to the structured node owning a node-scoped custom HTTP endpoint.</summary>
public interface IActivityNodeEndpointContext : IActivityEndpointContext
{
    /// <summary>Gets the active node to which the endpoint is scoped.</summary>
    IActivityNodeContext Node { get; }
    
    /// <summary>Binds a node-scoped endpoint context from the current route metadata.</summary>
    /// <param name="context">Current HTTP request.</param>
    /// <param name="parameter">Handler parameter being bound.</param>
    /// <returns>An endpoint context, or null if the route does not target an Activity.</returns>
    public new static ValueTask<IActivityNodeEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IActivityNodeEndpointContext?>(metadata is { BehaviorClass.Type: BehaviorType.Activity }
            ? metadata.ActivityNodeContext.Value = new ActivityNodeEndpointContext()
            : null);
    }
}