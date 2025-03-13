
using System.Text.Json;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Activities;

namespace Stateflows.Transport.REST;

internal class ActivityVisitor(
    RouteGroupBuilder behaviorsBuilder,
    Action<RouteHandlerBuilder> routeHandlerBuilderAction,
    ITypeMapper typeMapper
) : Activities.ActivityVisitor, IBehaviorClassVisitor, ITypeVisitor
{
    private readonly Dictionary<string, RouteGroupBuilder> RouteGroups = new();
    private readonly Dictionary<string, bool> Initializers = new();
    private readonly Dictionary<string, List<Type>> Triggers = new();
    private static readonly MethodInfo RegisterRequestMethod = typeof(ActivityVisitor).GetMethod(
        nameof(RegisterRequestEndpoint),
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

    private string CurrentActivityName = string.Empty;
    
    public void Visit<T>()
    {
        RegisterEventEndpoint<T>(CurrentActivityName);
    }

    public RouteGroupBuilder GetRouteGroup(string behaviorClassName)
    {
        if (!RouteGroups.TryGetValue(behaviorClassName, out var activity))
        {
            activity = behaviorsBuilder.MapGroup($"/{behaviorClassName}");
            RouteGroups.Add(behaviorClassName, activity);
            
            RegisterStandardEndpoints(behaviorClassName, activity);
        }

        return activity;
    }

    public override Task InitializerAddedAsync<TInitializationEvent>(string activityName, int activityVersion)
    {
        Initializers[activityName] = true;
        
        CurrentActivityName = activityName;
        typeMapper.VisitMappedTypes<TInitializationEvent>(this);
        CurrentActivityName = string.Empty;
        
        // RegisterEventEndpoint<TInitializationEvent>(activityName);

        return Task.CompletedTask;
    }

    public override Task AcceptEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName,
        string parentNodeName = null)
    {
        CurrentActivityName = activityName;
        typeMapper.VisitMappedTypes<TEvent>(this);
        CurrentActivityName = string.Empty;

        // RegisterEventEndpoint<TEvent>(activityName);

        return Task.CompletedTask;
    }

    public override Task ActivityAddedAsync(string activityName, int activityVersion)
    {
        RegisterRemainingEndpoints();
        
        return Task.CompletedTask;
    }

    public override Task ActivityTypeAddedAsync<TActivity>(string activityName, int activityVersion)
    {
        if (typeof(IActivityEndpoints).IsAssignableFrom(typeof(TActivity)))
        {
            var endpointsBuilder = new EndpointsBuilder(this, new ActivityClass(activityName));
            
            var activity = StateflowsActivator.CreateUninitializedInstance<TActivity>() as IActivityEndpoints;
            activity.RegisterEndpoints(endpointsBuilder);
        }
        
        return Task.CompletedTask;
    }

    public void RegisterEventEndpoint<TEvent>(string activityName)
    {
        var activity = GetRouteGroup(activityName);

        var eventType = typeof(TEvent);
        if (
            eventType.IsSubclassOf(typeof(SystemEvent)) ||
            eventType.IsSubclassOf(typeof(Exception))
        )
        {
            return;
        }

        if (!Triggers.TryGetValue(activityName, out var triggers))
        {
            triggers = new List<Type>();
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
                    [activityName, activity], 
                    null
                );
            }
            else
            {
                RegisterEventEndpoint<TEvent>(activityName, activity);
            }
        }
    }

    public void RegisterRemainingEndpoints()
    {
        foreach (var activityName in RouteGroups.Keys)
        {
            if (Initializers.TryGetValue(activityName, out var hasInitializers) && hasInitializers)
            {
                continue;
            }
            
            RegisterEventEndpoint<Initialize>(activityName);
        }
    }

    private static string GetEventName<TEvent>()
        => JsonNamingPolicy.CamelCase.ConvertName(Event<TEvent>.Name.Split('.').Last());

    private void RegisterEventEndpoint<TEvent>(string activityName, RouteGroupBuilder activity)
    {
        routeHandlerBuilderAction(
            activity.MapPost("/{instance}/" + GetEventName<TEvent>(),
                async (string instance, IActivityLocator locator, TEvent @event)
                    => locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior)
                        ? (await behavior.SendAsync(@event)).ToResult()
                        : Results.NotFound()
            )
        );
    }

    private void RegisterRequestEndpoint<TRequest, TResponse>(string activityName, RouteGroupBuilder activity)
        where TRequest : IRequest<TResponse>
    {
        routeHandlerBuilderAction(
            activity.MapPost("/{instance}/" + GetEventName<TRequest>(),
                async (string instance, IActivityLocator locator, TRequest request)
                    => locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior)
                        ? (await behavior.RequestAsync(request)).ToResult()
                        : Results.NotFound()
            )
        );
    }

    private void RegisterStandardEndpoints(string activityName, RouteGroupBuilder activity)
    {
        routeHandlerBuilderAction(
            activity.MapGet("/", async (IStateflowsStorage storage) =>
            {
                IEnumerable<BehaviorClass> actionClasses = [new ActivityClass(activityName)];
                var contexts = await storage.GetAllContextsAsync(actionClasses);
                return Results.Ok(contexts.Select(context => context.Id));
            })
        );

        routeHandlerBuilderAction(
            activity.MapGet("/{instance}/status", async (string instance, IActivityLocator locator)
                => locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior)
                    ? (await behavior.GetStatusAsync()).ToResult()
                    : Results.NotFound()
            )
        );

        routeHandlerBuilderAction(
            activity.MapGet(
                "/{instance}/notifications",
                async (
                    IActivityLocator locator,
                    string instance,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period
                ) =>
                {
                    period ??= TimeSpan.FromSeconds(60);
                    return locator.TryLocateActivity(new ActivityId(activityName, instance),
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
            activity.MapPost("/{instance}/finalize", async (string instance, IActivityLocator locator)
                => locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior)
                    ? (await behavior.FinalizeAsync()).ToResult()
                    : Results.NotFound()
            )
        );

        routeHandlerBuilderAction(
            activity.MapDelete("/{instance}", async (string instance, IActivityLocator locator)
                => locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior)
                    ? (await behavior.ResetAsync()).ToResult()
                    : Results.NotFound()
            )
        );
    }
}