using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.Extensions.MinimalAPIs;

public abstract class BehaviorVisitor(
    IEndpointRouteBuilder routeBuilder,
    ITypeMapper typeMapper
)
{
    protected readonly Dictionary<string, List<Type>> Triggers = new();
    
    private static readonly MethodInfo RegisterRequestMethod = typeof(ActivityVisitor).GetMethod(
        nameof(RegisterRequestEndpoint),
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;
    
    protected abstract void RegisterEventEndpoint<TEvent>(string behaviorName, IEndpointRouteBuilder activity, BehaviorStatus[]? supportedStatuses = null);

    protected abstract void RegisterRequestEndpoint<TRequest, TResponse>(string behaviorName, IEndpointRouteBuilder activity, BehaviorStatus[]? supportedStatuses = null)
        where TRequest : IRequest<TResponse>;
    
    protected void RegisterEventEndpoint<TEvent>(string activityName, BehaviorStatus[]? supportedStatuses = null)
    {
        var eventType = typeof(TEvent);
        if (
            eventType.GetCustomAttributes<NoApiMappingAttribute>().Any() ||
            eventType.IsAssignableTo(typeof(SystemEvent)) ||
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