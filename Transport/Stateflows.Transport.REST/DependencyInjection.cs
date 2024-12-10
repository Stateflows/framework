using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Stateflows.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Stateflows.Common.Utilities;
using System.Reflection;
using Stateflows.StateMachines;
using System.Threading.Tasks;

namespace Stateflows.Transport.REST
{
    public static class DependencyInjection
    {
        public static IEndpointRouteBuilder AddStateflowsRESTTransport(this IEndpointRouteBuilder builder, Action<RouteHandlerBuilder>? routeHandlerBuilderAction = null)
        {
            var root = builder.MapGroup("/stateflows");

            var behaviorClasses = root.MapGroup("/behaviorClasses");

            behaviorClasses.MapGet(
                "/",
                (IBehaviorClassesProvider provider) => Results.Ok(provider.AllBehaviorClasses)
            );

            behaviorClasses.MapGet(
                "/stateMachines",
                (IBehaviorClassesProvider provider) => Results.Ok(provider.AllBehaviorClasses.Where(behaviorClass => behaviorClass.Type == StateMachineClass.Type))
            );

            behaviorClasses.MapGet(
                "/activities",
                (IBehaviorClassesProvider provider) => Results.Ok(provider.AllBehaviorClasses.Where(behaviorClass => behaviorClass.Type == ActivityClass.Type))
            );

            var behaviors = root.MapGroup("/behaviors");

            var provider = builder.ServiceProvider.GetService<IBehaviorClassesProvider>();

            var inspector = builder.ServiceProvider.GetService<IBehaviorClassInspector>();

            var locator = builder.ServiceProvider.GetService<IBehaviorLocator>();

            MethodInfo SendAsyncMethod = typeof(IBehavior).GetMethod(nameof(IBehavior.SendAsync));

            MethodInfo RequestAsyncMethod = typeof(IBehavior).GetMethod(nameof(IBehavior.RequestAsync));

            var stateMachineClasses = provider.AllBehaviorClasses.Where(behaviorClass => behaviorClass.Type == StateMachineClass.Type);

            var stateMachines = behaviors.MapGroup("/stateMachines");

            foreach (var behaviorClass in stateMachineClasses)
            {
                var stateMachine = stateMachines.MapGroup($"/{behaviorClass.Name}");

                stateMachine.MapGet("/{instance}/status", (string instance) =>
                {
                });

                stateMachine.MapGet("/{instance}/currentState", async (string instance) =>
                {
                    if (locator.TryLocateStateMachine(new BehaviorId(behaviorClass, instance), out var behavior))
                    {
                        return Results.Ok(await behavior.GetStatusAsync());
                    }

                    return Results.NotFound();
                });

                var inspection = inspector.Inspect(behaviorClass);

                foreach (var type in inspection.InitializationEventTypes)
                {
                    var eventName = Event.GetName(type);

                    stateMachine.MapPost("/{instance}/" + eventName, async (string instance, [FromBody] string body) =>
                    {
                        var @event = StateflowsJsonConverter.DeserializeObject(body, type);

                        if (locator.TryLocateBehavior(new BehaviorId(behaviorClass, instance), out var behavior))
                        {
                            if (type.IsRequest())
                            {
                                var task = (Task<SendResult>)SendAsyncMethod.Invoke(behavior, new object[] { @event, null });
                                var result = await task;

                                return Results.Ok(result);
                            }
                            else
                            {
                                //? RequestAsyncMethod.Invoke(behavior, new object[] { @event, null })
                                //return Results.Ok(await task);
                            }
                        }

                        return Results.NotFound();
                    });
                }
            }

            var activities = behaviors.MapGroup("/activities");

            return builder;
        }
    }
}