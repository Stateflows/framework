using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Stateflows.Common;
using Stateflows.Actions;
using Stateflows.Common.Interfaces;

namespace Stateflows.Transport.REST;

internal class ActionVisitor(RouteGroupBuilder behaviorsBuilder, System.Action<RouteHandlerBuilder> routeHandlerBuilderAction) : Actions.ActionVisitor, IBehaviorClassVisitor
{
    private readonly Dictionary<string, RouteGroupBuilder> RouteGroups = new();

    public RouteGroupBuilder GetRouteGroup(string behaviorClassName)
    {
        if (!RouteGroups.TryGetValue(behaviorClassName, out var action))
        {
            action = behaviorsBuilder.MapGroup($"/{behaviorClassName}");
            RouteGroups.Add(behaviorClassName, action);
            
            RegisterStandardEndpoints(behaviorClassName, action);
        }

        return action;
    }

    public override Task ActionAddedAsync(string actionName, int actionVersion)
    {
        GetRouteGroup(actionName);
        
        return Task.CompletedTask;
    }

    private static string GetEventName<TEvent>()
        => JsonNamingPolicy.CamelCase.ConvertName(Event<TEvent>.Name.Split('.').Last());

    private void RegisterStandardEndpoints(string actionName, RouteGroupBuilder action)
    {
        routeHandlerBuilderAction(
            action.MapGet("/", async (IStateflowsStorage storage) =>
            {
                IEnumerable<BehaviorClass> actionClasses = [new ActionClass(actionName)];
                var contexts = await storage.GetAllContextsAsync(actionClasses);
                return Results.Ok(contexts.Select(context => context.Id));
            })
        );
        
        routeHandlerBuilderAction(
            action.MapGet("/{instance}/status", async (string instance, IActionLocator locator)
                => locator.TryLocateAction(new ActionId(actionName, instance), out var behavior)
                    ? (await behavior.GetStatusAsync()).ToResult()
                    : Results.NotFound()
            )
        );

        routeHandlerBuilderAction(
            action.MapPost("/{instance}/initialize", async (string instance, IActionLocator locator)
                => locator.TryLocateAction(new ActionId(actionName, instance), out var behavior)
                    ? (await behavior.SendAsync(new Initialize())).ToResult()
                    : Results.NotFound()
            )
        );

        routeHandlerBuilderAction(
            action.MapGet(
                "/{instance}/notifications",
                async (
                    IActionLocator locator,
                    string instance,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period
                ) =>
                {
                    period ??= TimeSpan.FromSeconds(60);
                    return locator.TryLocateAction(new ActionId(actionName, instance),
                        out var behavior)
                        ? (await behavior.RequestAsync(new NotificationsRequest()
                        {
                            Period = (TimeSpan)period,
                            NotificationNames = names.ToList()
                        })).ToResult()
                        : Results.NotFound();
                })
        );

        routeHandlerBuilderAction(
            action.MapPost("/{instance}/finalize", async (string instance, IActionLocator locator)
                => locator.TryLocateAction(new ActionId(actionName, instance), out var behavior)
                    ? (await behavior.FinalizeAsync()).ToResult()
                    : Results.NotFound()
            )
        );

        routeHandlerBuilderAction(
            action.MapDelete("/{instance}", async (string instance, IActionLocator locator)
                => locator.TryLocateAction(new ActionId(actionName, instance), out var behavior)
                    ? (await behavior.ResetAsync()).ToResult()
                    : Results.NotFound()
            )
        );
    }
}