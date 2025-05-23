using System.ComponentModel.DataAnnotations;
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
using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.Extensions.MinimalAPIs;

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

    public Dictionary<string, List<HateoasLink>> CustomHateoasLinks { get; set; } = new();
    public void AddLink(HateoasLink link, string scope = "")
    {
        if (!CustomHateoasLinks.TryGetValue(scope, out var links))
        {
            links = new List<HateoasLink>();
            CustomHateoasLinks.Add(scope, links);
        }
        
        links.Add(link);
    }

    public override Task InitializerAddedAsync<TInitializationEvent>(string activityName, int activityVersion)
    {
        Initializers[activityName] = true;
        
        CurrentActivityName = activityName;
        typeMapper.VisitMappedTypes<TInitializationEvent>(this);
        CurrentActivityName = string.Empty;

        return Task.CompletedTask;
    }

    public override Task AcceptEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName,
        string? parentNodeName = null)
    {
        CurrentActivityName = activityName;
        typeMapper.VisitMappedTypes<TEvent>(this);
        CurrentActivityName = string.Empty;

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
            
            var activity = (IActivityEndpoints)StateflowsActivator.CreateUninitializedInstance<TActivity>();
            activity.RegisterEndpoints(endpointsBuilder);
        }
        
        return Task.CompletedTask;
    }

    private void RegisterEventEndpoint<TEvent>(string activityName, RouteGroupBuilder activity)
        => activity.RegisterEventEndpoint<TEvent>(routeHandlerBuilderAction,
            BehaviorType.Activity, activityName, CustomHateoasLinks);

    private void RegisterRequestEndpoint<TRequest, TResponse>(string activityName, RouteGroupBuilder activity)
        where TRequest : IRequest<TResponse>
        => activity.RegisterRequestEndpoint<TRequest, TResponse>(routeHandlerBuilderAction,
            BehaviorType.Activity, activityName, CustomHateoasLinks);

    private void RegisterEventEndpoint<TEvent>(string activityName)
    {
        var activity = GetRouteGroup(activityName);

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

    private void RegisterRemainingEndpoints()
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
        => JsonNamingPolicy.CamelCase.ConvertName(Event<TEvent>.Name.ToShortName());

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
            activity.MapGet("/{instance}/status",
                async (
                    string instance,
                    IActivityLocator locator
                ) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        var result = await behavior.GetStatusAsync([new NoImplicitInitialization()]);
                        // workaround for return code 200 regardless behavior actual status
                        result.Status = EventStatus.Consumed;
                        return result.ToResult([], result.Response, CustomHateoasLinks);
                    }

                    return Results.NotFound();
                }
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
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        var result = await behavior.GetNotificationsAsync(names, period);
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult(result.Response.Notifications, behaviorInfo, CustomHateoasLinks);
                    }
                    return Results.NotFound();
                })
        );

        routeHandlerBuilderAction(
            activity.MapPost("/{instance}/finalize",
                async (
                    string instance,
                    IActivityLocator locator
                ) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        var result = await behavior.FinalizeAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, CustomHateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
        );

        routeHandlerBuilderAction(
            activity.MapDelete("/{instance}", 
                async (string instance, IActivityLocator locator) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        var result = await behavior.ResetAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, CustomHateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
        );
    }

    public override Task NodeTypeAddedAsync<TNode>(string activityName, int activityVersion, string nodeName)
    {
        if (typeof(IStructuredActivityNodeEndpoints).IsAssignableFrom(typeof(TNode)))
        {
            var behaviorClass = new ActivityClass(activityName);
            
            DependencyInjection.ActivityEndpointBuilders.Add(visitor =>
            {
                var builder = new EndpointsBuilder(visitor, behaviorClass, nodeName);
                
                var node = (IStructuredActivityNodeEndpoints)StateflowsActivator.CreateUninitializedInstance<TNode>();
                node.RegisterEndpoints(builder);
            });
        }

        return Task.CompletedTask;
    }
}