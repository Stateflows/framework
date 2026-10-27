using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

/// <summary>Exposes behavior operations and response construction inside a custom HTTP endpoint.</summary>
/// <remarks>The endpoint filter supplies the addressed behavior context for the current request.
/// This is a request-scoped context, not a durable behavior handle.</remarks>
public interface IBehaviorEndpointContext : IBehaviorLocator
{
    /// <summary>Gets the addressed behavior's execution context.</summary>
    IBehaviorContext Behavior { get; }
    
    /// <summary>Binds a handler parameter using the Stateflows endpoint metadata.</summary>
    /// <param name="context">Current HTTP request.</param>
    /// <param name="parameter">Handler parameter being bound.</param>
    /// <returns>A request-scoped endpoint context, or null when Stateflows metadata is absent.</returns>
    public static ValueTask<IBehaviorEndpointContext?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<EndpointMetadata>();
        return ValueTask.FromResult<IBehaviorEndpointContext?>(metadata != null
            ? metadata.BehaviorContext.Value = new BehaviorEndpointContext()
            : null);
    }

    /// <summary>Creates a response containing the behavior's links and metadata.</summary>
    /// <returns>A response envelope without an application result.</returns>
    EndpointResponse Response();

    /// <summary>Creates a response containing an application result, behavior links, and metadata.</summary>
    /// <typeparam name="T">Application result type.</typeparam>
    /// <param name="result">Application result to include.</param>
    /// <returns>A typed response envelope.</returns>
    EndpointResponse<T> Response<T>(T result);
}