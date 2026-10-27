using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Provides deduplication and CLR-event classification for HTTP endpoint discovery visitors.</summary>
/// <param name="routeBuilder">Route builder receiving generated endpoints.</param>
/// <param name="typeMapper">Type mapper supplied by the behavior registration infrastructure.</param>
public abstract class BehaviorVisitor(
    IEndpointRouteBuilder routeBuilder,
    ITypeMapper typeMapper
)
{
    /// <summary>Records CLR event types already mapped for each behavior name.</summary>
    protected readonly Dictionary<string, List<Type>> Triggers = new();
    /// <summary>Records structural event names already mapped for each behavior name.</summary>
    protected readonly HashSet<(string BehaviorName, string EventName)> StructuralTriggers = new();
    
    private static readonly MethodInfo RegisterRequestMethod = typeof(ActivityVisitor).GetMethod(
        nameof(RegisterRequestEndpoint),
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;
    
    /// <summary>Maps a non-request CLR event endpoint for a behavior class.</summary>
    /// <typeparam name="TEvent">Event payload type.</typeparam>
    /// <param name="behaviorName">Registered behavior name.</param>
    /// <param name="activity">Route builder receiving the endpoint.</param>
    /// <param name="supportedStatuses">Optional lifecycle statuses in which to advertise the endpoint.</param>
    protected abstract void RegisterEventEndpoint<TEvent>(string behaviorName, IEndpointRouteBuilder activity, BehaviorStatus[]? supportedStatuses = null);

    /// <summary>Maps a CLR request endpoint with a typed response.</summary>
    /// <typeparam name="TRequest">Request payload type.</typeparam>
    /// <typeparam name="TResponse">Response payload type.</typeparam>
    /// <param name="behaviorName">Registered behavior name.</param>
    /// <param name="activity">Route builder receiving the endpoint.</param>
    /// <param name="supportedStatuses">Optional lifecycle statuses in which to advertise the endpoint.</param>
    protected abstract void RegisterRequestEndpoint<TRequest, TResponse>(string behaviorName, IEndpointRouteBuilder activity, BehaviorStatus[]? supportedStatuses = null)
        where TRequest : IRequest<TResponse>;
    
    /// <summary>Discovers and maps a CLR event or request once per behavior class.</summary>
    /// <typeparam name="TEvent">Event or request payload type.</typeparam>
    /// <param name="activityName">Registered behavior name.</param>
    /// <param name="supportedStatuses">Optional lifecycle statuses in which to advertise the endpoint.</param>
    /// <remarks>System events, exceptions, structural envelopes, and events marked with
    /// <see cref="NoApiMappingAttribute"/> are excluded.</remarks>
    protected void RegisterEventEndpoint<TEvent>(string activityName, BehaviorStatus[]? supportedStatuses = null)
    {
        var eventType = typeof(TEvent);
        if (
            eventType.GetCustomAttributes<NoApiMappingAttribute>().Any() ||
            eventType.IsAssignableTo(typeof(SystemEvent)) ||
            eventType.IsAssignableTo(typeof(StructuralEvent)) ||
            eventType.IsAssignableTo(typeof(Exception))
        )
        {
            return;
        }

        if (!Triggers.TryGetValue(activityName, out var triggers))
        {
            triggers = [];
            Triggers.Add(activityName, triggers);
        }

        if (!triggers.Contains(typeof(TEvent)))
        {
            triggers.Add(eventType);

            if (eventType.IsRequest())
            {
                var responseType = eventType
                    .GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                    .GetGenericArguments()
                    .First();

                var method = RegisterRequestMethod.MakeGenericMethod(eventType, responseType);
                method.Invoke(
                    this,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    [activityName, routeBuilder, supportedStatuses],
                    null
                );
            }
            else
            {
                RegisterEventEndpoint<TEvent>(activityName, routeBuilder, supportedStatuses);
            }
        }
    }
}