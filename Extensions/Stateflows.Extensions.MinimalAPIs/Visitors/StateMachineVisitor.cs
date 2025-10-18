using System.Reflection;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Extensions.MinimalAPIs.Attributes;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

internal class StateMachineVisitor(
    IEndpointRouteBuilder routeBuilder,
    Interceptor interceptor,
    ITypeMapper typeMapper
) : StateMachines.StateMachineVisitor, IBehaviorClassVisitor, ITypeVisitor
{
    public IEndpointRouteBuilder RouteBuilder => routeBuilder;
    public Interceptor Interceptor => interceptor;
    private readonly Dictionary<string, bool> Initializers = new();
    private readonly Dictionary<string, List<Type>> Triggers = new();
    private static readonly MethodInfo RegisterRequestMethod = typeof(StateMachineVisitor).GetMethod(
        nameof(RegisterRequestEndpoint),
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

    private string CurrentStateMachineName = string.Empty;
    private BehaviorStatus[] SupportedStatuses = [];
    
    public void Visit<T>()
    {
        RegisterEventEndpoint<T>(CurrentStateMachineName);
    }

    public Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> HateoasLinks { get; set; } = new();

    public override Task InitializerAddedAsync<TInitializationEvent>(string stateMachineName, int stateMachineVersion)
    {
        Initializers[stateMachineName] = true;
        
        CurrentStateMachineName = stateMachineName;
        SupportedStatuses = [BehaviorStatus.NotInitialized];
        typeMapper.VisitMappedTypes<TInitializationEvent>(this);
        CurrentStateMachineName = string.Empty;
        SupportedStatuses = [];

        return Task.CompletedTask;
    }

    public override Task TransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName,
        string targetVertexName = null, bool isElse = false)
    {
        CurrentStateMachineName = stateMachineName;
        SupportedStatuses = [BehaviorStatus.Initialized];
        typeMapper.VisitMappedTypes<TEvent>(this);
        CurrentStateMachineName = string.Empty;
        SupportedStatuses = [];

        return Task.CompletedTask;
    }

    public override Task StateMachineAddedAsync(string stateMachineName, int stateMachineVersion)
    {
        RegisterStandardEndpoints(stateMachineName);
        RegisterRemainingEndpoints(stateMachineName);
        
        return Task.CompletedTask;
    }

    private void RegisterEventEndpoint<TEvent>(string stateMachineName)
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

        if (!Triggers.TryGetValue(stateMachineName, out var triggers))
        {
            triggers = new List<Type>();
            Triggers.Add(stateMachineName, triggers);
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
                    [stateMachineName, routeBuilder], 
                    null
                );
            }
            else
            {
                RegisterEventEndpoint<TEvent>(stateMachineName, routeBuilder);
            }
        }
    }

    private void RegisterRemainingEndpoints(string stateMachineName)
    {
        if (Initializers.TryGetValue(stateMachineName, out var hasInitializers) && hasInitializers)
        {
            return;
        }
        
        RegisterEventEndpoint<Initialize>(stateMachineName);
    }

    private void RegisterEventEndpoint<TEvent>(string stateMachineName, IEndpointRouteBuilder stateMachine)
        => stateMachine.RegisterEventEndpoint<TEvent>(interceptor,
            BehaviorType.StateMachine, stateMachineName, HateoasLinks);

    private void RegisterRequestEndpoint<TRequest, TResponse>(string stateMachineName, IEndpointRouteBuilder stateMachine)
        where TRequest : IRequest<TResponse>
        => stateMachine.RegisterRequestEndpoint<TRequest, TResponse>(interceptor,
            BehaviorType.StateMachine, stateMachineName, HateoasLinks);

    private void RegisterStandardEndpoints(string stateMachineName)
    {
        var behaviorClass = new StateMachineClass(stateMachineName);
        
        var method = HttpMethods.Get;
        var route = $"/stateMachines/{stateMachineName}";
        if (interceptor.BeforeGetInstancesEndpointDefinition(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(route, [method], async (IStateflowsStorage storage) =>
            {
                BehaviorClass[] actionClasses = [new StateMachineClass(stateMachineName)];
                var contextIds = await storage.GetAllContextIdsAsync(actionClasses);
                return Results.Ok(contextIds.Select(id => new { Id = id }));
            })
            .WithTags($"{BehaviorType.StateMachine} {stateMachineName}");

            interceptor.AfterGetInstancesEndpointDefinition(behaviorClass, method, route, routeHandlerBuilder);
        }

        route = $"/stateMachines/{stateMachineName}/{{instance}}/status";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<StateMachineInfoRequest>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IStateMachineLocator locator,
                    HttpContext httpContext,
                    [FromQuery] bool implicitInitialization = false,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                    {
                        if (stream)
                        {
                            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");
                            
                            await using var watcher = await behavior.WatchAsync(
                                [ Event<StateMachineInfo>.Name ],
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
                            var result =
                                await behavior.GetStatusAsync(implicitInitialization
                                    ? []
                                    : [new NoImplicitInitialization()]);
                            // workaround for return code 200 regardless behavior actual status
                            result.Status = EventStatus.Consumed;
                            return result.ToResult([], result.Response, HateoasLinks);
                        }
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.StateMachine} {stateMachineName}");

            interceptor.AfterEventEndpointDefinition<StateMachineInfoRequest>(behaviorClass, method, route, routeHandlerBuilder);
            
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

        route = $"/stateMachines/{stateMachineName}/{{instance}}/notifications";
        method = HttpMethods.Get;
        if (interceptor.BeforeEventEndpointDefinition<NotificationsRequest>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IStateMachineLocator locator,
                    HttpContext httpContext,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period,
                    [FromQuery] bool stream = false
                ) =>
                {
                    if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
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
                            
                            var notifications = (await behavior.GetNotificationsAsync(names, DateTime.Now - period.Value)).ToArray();
                            var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                            
                            var result = new SendResult(EventStatus.Consumed, new EventValidation(true));
                            return result.ToResult(notifications, behaviorInfo, HateoasLinks);
                        }
                    }

                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.StateMachine} {stateMachineName}");
            
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

        route = $"/stateMachines/{stateMachineName}/{{instance}}/finalize";
        method = HttpMethods.Post;
        if (interceptor.BeforeEventEndpointDefinition<Finalize>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IStateMachineLocator locator
                ) =>
                {
                    if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                    {
                        // var compoundResult = await behavior.SendCompoundAsync(b => b
                        //     .Add(new Finalize())
                        //     .Add(new StateMachineInfoRequest(), [new NoImplicitInitialization()])
                        // );
                        //
                        // var result = compoundResult.Response.Results.First();
                        // var behaviorInfo = ((EventHolder<StateMachineInfo>)compoundResult.Response.Results.Last().Response).Payload;
                        
                        var result = await behavior.FinalizeAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, HateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.StateMachine} {stateMachineName}");
            
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

        route = $"/stateMachines/{stateMachineName}/{{instance}}";
        method = HttpMethods.Delete;
        if (interceptor.BeforeEventEndpointDefinition<Reset>(behaviorClass, ref method, ref route))
        {
            var routeHandlerBuilder = routeBuilder.MapMethods(
                route,
                [method],
                async (
                    string instance,
                    IStateMachineLocator locator
                ) =>
                {
                    if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                    {
                        // var compoundResult = await behavior.SendCompoundAsync(b => b
                        //     .Add(new Reset())
                        //     .Add(new StateMachineInfoRequest(), [new NoImplicitInitialization()])
                        // );
                        //
                        // var result = compoundResult.Response.Results.First();
                        // var behaviorInfo = ((EventHolder<StateMachineInfo>)compoundResult.Response.Results.Last().Response).Payload;
                        
                        var result = await behavior.ResetAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, HateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            )
            .WithTags($"{BehaviorType.StateMachine} {stateMachineName}");
            
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

    public override Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineName, int stateMachineVersion)
    {
        var stateMachineType = typeof(TStateMachine);
        if (typeof(IStateMachineEndpoints).IsAssignableFrom(stateMachineType))
        {
            var endpointsBuilder = new EndpointsBuilder(routeBuilder, this, interceptor, new StateMachineClass(stateMachineName));

            stateMachineType.CallStaticMethod(nameof(IStateMachineEndpoints.RegisterEndpoints), [ typeof(IEndpointsBuilder) ], [ endpointsBuilder ]);
            
            // RegisterEndpoints<TStateMachine>(endpointsBuilder);

            // var endpointsBuilder = new EndpointsBuilder(routeBuilder, this, interceptor, new StateMachineClass(stateMachineName));
            //
            // var stateMachine = (IStateMachineEndpoints)StateflowsActivator.CreateModelElementInstanceAsync<TStateMachine>(serviceProvider);
            // stateMachine.RegisterEndpoints(endpointsBuilder);
        }
        
        return Task.CompletedTask;
    }

    private static void RegisterEndpoints<TEndpointsOwner>(EndpointsBuilder endpointsBuilder)
    {
        var smType = typeof(TEndpointsOwner);
        var staticRegister = smType.GetMethod(
            nameof(IStateMachineEndpoints.RegisterEndpoints),
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [ typeof(EndpointsBuilder) ],
            modifiers: null
        );

        staticRegister.Invoke(null, [ endpointsBuilder ]);
    }

    public override Task VertexTypeAddedAsync<TVertex>(string stateMachineName, int stateMachineVersion, string vertexName)
    {
        var vertexType = typeof(TVertex);
        if (typeof(IStateEndpoints).IsAssignableFrom(vertexType))
        {
            var behaviorClass = new StateMachineClass(stateMachineName);
            
            DependencyInjection.StateMachineEndpointBuilders.Add(visitor =>
            {
                var endpointsBuilder = new EndpointsBuilder(routeBuilder, visitor, interceptor, behaviorClass, vertexName);
                
                vertexType.CallStaticMethod(nameof(IStateEndpoints.RegisterEndpoints), [ typeof(IEndpointsBuilder) ], [ endpointsBuilder ]);
                // RegisterEndpoints<TVertex>(endpointsBuilder);
                
                // var state = (IStateEndpoints)StateflowsActivator.CreateUninitializedInstance<TVertex>();
                // state.RegisterEndpoints(endpointsBuilder);
            });
        }

        return Task.CompletedTask;
    }

    public override Task CustomEventAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, BehaviorStatus[] supportedStatuses)
    {
        CurrentStateMachineName = stateMachineName;
        SupportedStatuses = supportedStatuses;
        typeMapper.VisitMappedTypes<TEvent>(this);
        CurrentStateMachineName = string.Empty;
        SupportedStatuses = [];

        return Task.CompletedTask;
    }
}