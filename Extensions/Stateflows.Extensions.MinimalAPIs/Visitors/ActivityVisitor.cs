using System.Text.Json;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Net.Http.Headers;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Activities;
using Stateflows.Extensions.MinimalAPIs.Attributes;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ActivityVisitor(
    IEndpointRouteBuilder routeBuilder,
    Interceptor interceptor,
    ITypeMapper typeMapper
) : Activities.ActivityVisitor, IBehaviorClassVisitor, ITypeVisitor
{
    public IEndpointRouteBuilder RouteBuilder => routeBuilder;
    public Interceptor Interceptor => interceptor;
    private readonly Dictionary<string, bool> Initializers = new();
    private readonly Dictionary<string, List<Type>> Triggers = new();
    private static readonly MethodInfo RegisterRequestMethod = typeof(ActivityVisitor).GetMethod(
        nameof(RegisterRequestEndpoint),
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

    private string CurrentActivityName = string.Empty;
    private BehaviorStatus[] SupportedStatuses = [];
    
    public void Visit<T>()
    {
        RegisterEventEndpoint<T>(CurrentActivityName);
    }

    public Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> HateoasLinks { get; set; } = new();

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
        RegisterStandardEndpoints(activityName);
        RegisterRemainingEndpoints(activityName);
        
        return Task.CompletedTask;
    }

    // private static void RegisterEndpoints<TEndpointsOwner>(EndpointsBuilder endpointsBuilder)
    // {
    //     var activityType = typeof(TEndpointsOwner);
    //     var staticRegister = activityType.GetMethod(
    //         nameof(IActivityEndpoints.RegisterEndpoints),
    //         BindingFlags.Public | BindingFlags.Static,
    //         binder: null,
    //         types: [ typeof(EndpointsBuilder) ],
    //         modifiers: null
    //     );
    //
    //     staticRegister.Invoke(null, [ endpointsBuilder ]);
    // }

    public override Task ActivityTypeAddedAsync<TActivity>(string activityName, int activityVersion)
    {
        var activityType = typeof(TActivity);
        if (typeof(IActivityEndpoints).IsAssignableFrom(activityType))
        {
            var endpointsBuilder = new EndpointsBuilder(routeBuilder, this, interceptor, new ActivityClass(activityName));
            
            activityType.CallStaticMethod(nameof(IActivityEndpoints.RegisterEndpoints), [ typeof(IEndpointsBuilder) ], [ endpointsBuilder ]);

            // RegisterEndpoints<TActivity>(endpointsBuilder);
            
            // var activity = (IActivityEndpoints)StateflowsActivator.CreateUninitializedInstance<TActivity>();
            // activity.RegisterEndpoints(endpointsBuilder);
        }
        
        return Task.CompletedTask;
    }

    private void RegisterEventEndpoint<TEvent>(string activityName, IEndpointRouteBuilder activity)
        => activity.RegisterEventEndpoint<TEvent>(interceptor,
            BehaviorType.Activity, activityName, HateoasLinks);

    private void RegisterRequestEndpoint<TRequest, TResponse>(string activityName, IEndpointRouteBuilder activity)
        where TRequest : IRequest<TResponse>
        => activity.RegisterRequestEndpoint<TRequest, TResponse>(interceptor,
            BehaviorType.Activity, activityName, HateoasLinks);
    
    private void RegisterEventEndpoint<TEvent>(string activityName)
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
                    [activityName, routeBuilder], 
                    null
                );
            }
            else
            {
                RegisterEventEndpoint<TEvent>(activityName, routeBuilder);
            }
        }
    }

    private void RegisterRemainingEndpoints(string activityName)
    {
        if (Initializers.TryGetValue(activityName, out var hasInitializers) && hasInitializers)
        {
            return;
        }
        
        RegisterEventEndpoint<Initialize>(activityName);
    }

    private static string GetEventName<TEvent>()
        => JsonNamingPolicy.CamelCase.ConvertName(Event<TEvent>.Name.ToShortName());

    private void RegisterStandardEndpoints(string activityName)
    {
        var behaviorClass = new ActivityClass(activityName);
        
        var method = HttpMethods.Get;
        var route = $"/activities/{activityName}";
        if (interceptor.BeforeGetInstancesEndpointDefinition(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(route, [method], async (IStateflowsStorage storage) =>
            {
                BehaviorClass[] actionClasses = [behaviorClass];
                var contextIds = await storage.GetAllContextIdsAsync(actionClasses);
                return Results.Ok(contextIds.Select(id => new { Id = id }));
            })
            .WithTags($"{BehaviorType.Activity} {activityName}");

            interceptor.AfterGetInstancesEndpointDefinition(behaviorClass, method, route, routeHandlerBuilder);
        }

        route = $"/activities/{activityName}/{{instance}}/status";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<ActivityInfoRequest>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActivityLocator locator,
                    HttpContext httpContext,
                    [FromQuery] bool implicitInitialization = false,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        if (stream)
                        {
                            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");
                            
                            await using var watcher = await behavior.WatchAsync(
                                [ Event<ActivityInfo>.Name ],
                                async eventHolder => await httpContext.WriteEventAsync(eventHolder)
                            );

                            while (!httpContext.RequestAborted.IsCancellationRequested)
                            {
                                await Task.Delay(1000);
                            }

                            return Results.Empty;
                        }
                        else
                        {
                            var requestResult =
                                await behavior.GetStatusAsync(implicitInitialization
                                    ? []
                                    : [new NoImplicitInitialization()]);
                            // workaround for return code 200 regardless behavior actual status
                            requestResult.Status = EventStatus.Consumed;
                            return requestResult.ToResult([], requestResult.Response, HateoasLinks);
                        }
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");

            interceptor.AfterEventEndpointDefinition<ActivityInfoRequest>(behaviorClass, method, route, routeHandlerBuilder);
            
            HateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = "status",
                    Href = route,
                    Method = method
                },
                [BehaviorStatus.NotInitialized, BehaviorStatus.Initialized, BehaviorStatus.Finalized]
            );
        }

        route = $"/activities/{activityName}/{{instance}}/notifications";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<NotificationsRequest>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActivityLocator locator,
                    HttpContext httpContext,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        if (stream)
                        {
                            period ??= TimeSpan.FromSeconds(0);
                            
                            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");
                            
                            await using var watcher = await behavior.WatchAsync(
                                names,
                                async eventHolder => await httpContext.WriteEventAsync(eventHolder),
                                DateTime.Now - period.Value
                            );

                            while (!httpContext.RequestAborted.IsCancellationRequested)
                            {
                                await Task.Delay(1000);
                            }

                            return Results.Empty;
                        }
                        else
                        {
                            period ??= TimeSpan.FromSeconds(60);
                            var notifications = (await behavior.GetNotificationsAsync(names, DateTime.Now - period))
                                .ToArray();
                            var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()]))
                                .Response;
                            var sendResult = new SendResult(EventStatus.Consumed, new EventValidation(true));
                            return sendResult.ToResult(notifications, behaviorInfo, HateoasLinks);
                        }
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");
            
            interceptor.AfterEventEndpointDefinition<NotificationsRequest>(behaviorClass, method, route, routeHandlerBuilder);
            
            HateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = "notifications",
                    Href = route,
                    Method = method
                },
                [BehaviorStatus.Initialized, BehaviorStatus.Finalized]
            );
        }

        route = $"/activities/{activityName}/{{instance}}/finalize";
        method = HttpMethods.Post;
        if (interceptor.BeforeEventEndpointDefinition<Finalize>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IActivityLocator locator
                ) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        // var compoundResult = await behavior.SendCompoundAsync(b => b
                        //     .Add(new Finalize())
                        //     .Add(new ActivityInfoRequest(), [new NoImplicitInitialization()])
                        // );
                        //
                        // var result = compoundResult.Response.Results.First();
                        // var behaviorInfo = ((EventHolder<ActivityInfo>)compoundResult.Response.Results.Last().Response).Payload;
                        //
                        // return result.ToResult([], behaviorInfo, HateoasLinks);
                        
                        var sendResult = await behavior.FinalizeAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return sendResult.ToResult([], behaviorInfo, HateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");
            
            interceptor.AfterEventEndpointDefinition<Finalize>(behaviorClass, method, route, routeHandlerBuilder);
            
            HateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = "finalize",
                    Href = route,
                    Method = method
                },
                [BehaviorStatus.Initialized]
            );
        }

        route = $"/activities/{activityName}/{{instance}}";
        method = HttpMethods.Delete;
        if (interceptor.BeforeEventEndpointDefinition<Reset>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (string instance, IActivityLocator locator) =>
                {
                    if (locator.TryLocateActivity(new ActivityId(activityName, instance), out var behavior))
                    {
                        // var compoundResult = await behavior.SendCompoundAsync(b => b
                        //     .Add(new Reset())
                        //     .Add(new BehaviorInfoRequest(), [new NoImplicitInitialization()])
                        // );
                        //
                        // var result = compoundResult.Response.Results.First();
                        // var behaviorInfo = ((EventHolder<BehaviorInfo>)compoundResult.Response.Results.Last().Response).Payload;
                        //
                        // return result.ToResult([], behaviorInfo, HateoasLinks);
                        
                        var sendResult = await behavior.ResetAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return sendResult.ToResult([], behaviorInfo, HateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.Activity} {activityName}");
            
            interceptor.AfterEventEndpointDefinition<Reset>(behaviorClass, method, route, routeHandlerBuilder);
            
            HateoasLinks.AddLink(
                behaviorClass.Name,
                new HateoasLink()
                {
                    Rel = "reset",
                    Href = route,
                    Method = method
                },
                [BehaviorStatus.Initialized, BehaviorStatus.Finalized]
            );
        }
    }

    public override Task NodeTypeAddedAsync<TNode>(string activityName, int activityVersion, string nodeName)
    {
        var nodeType = typeof(TNode);
        if (typeof(IStructuredActivityNodeEndpoints).IsAssignableFrom(nodeType))
        {
            var behaviorClass = new ActivityClass(activityName);
            
            DependencyInjection.ActivityEndpointBuilders.Add(visitor =>
            {
                var endpointsBuilder = new EndpointsBuilder(routeBuilder, visitor, interceptor, behaviorClass, nodeName);
                
                nodeType.CallStaticMethod(nameof(IStructuredActivityNodeEndpoints.RegisterEndpoints), [ typeof(IEndpointsBuilder) ], [ endpointsBuilder ]);
                // RegisterEndpoints<TNode>(endpointsBuilder);
                
                // var node = (IStructuredActivityNodeEndpoints)StateflowsActivator.CreateUninitializedInstance<TNode>();
                // node.RegisterEndpoints(endpointsBuilder);
            });
        }

        return Task.CompletedTask;
    }

    public override Task CustomEventAddedAsync<TEvent>(string activityName, int activityVersion, BehaviorStatus[] supportedStatuses)
    {
        CurrentActivityName = activityName;
        typeMapper.VisitMappedTypes<TEvent>(this);
        CurrentActivityName = string.Empty;

        return Task.CompletedTask;
    }
}