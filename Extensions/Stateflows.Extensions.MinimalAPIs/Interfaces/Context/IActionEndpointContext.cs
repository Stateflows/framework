using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Actions;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

/// <summary>Provides request-scoped access to an Action from a custom HTTP endpoint.</summary>
public interface IActionEndpointContext : IBehaviorEndpointContext
{
    /// <summary>Binds an Action endpoint context from the current route metadata.</summary>
    /// <param name="context">Current HTTP request.</param>
    /// <param name="parameter">Handler parameter being bound.</param>
    /// <returns>An endpoint context, or null if the route does not target an Action.</returns>
    public new static ValueTask<IActionEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IActionEndpointContext?>(metadata is { BehaviorClass.Type: BehaviorType.Action }
            ? metadata.ActionContext.Value = new ActionEndpointContext()
            : null);
    }
}