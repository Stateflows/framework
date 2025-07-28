using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Stateflows.Common;
using Stateflows.Common.Classes;
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
            });

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
                    IStateMachineLocator locator
                ) =>
                {
                    if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                    {
                        var result = await behavior.GetStatusAsync([new NoImplicitInitialization()]);
                        // workaround for return code 200 regardless behavior actual status
                        result.Status = EventStatus.Consumed;
                        return result.ToResult([], result.Response, HateoasLinks);
                    }

                    return Results.NotFound();
                }
            );

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
                    IStateMachineLocator locator,
                    string instance,
                    [FromQuery] string[] names,
                    [FromQuery] TimeSpan? period
                ) =>
                {
                    period ??= TimeSpan.FromSeconds(60);
                    if (locator.TryLocateStateMachine(new StateMachineId(stateMachineName, instance), out var behavior))
                    {
                        var result = await behavior.GetNotificationsAsync(names, period, [new NoImplicitInitialization()]);
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult(result.Response.Notifications, behaviorInfo, HateoasLinks);
                    }
                    return Results.NotFound();
                });
            
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
                        var result = await behavior.FinalizeAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, HateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            );
            
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
                        var result = await behavior.ResetAsync();
                        var behaviorInfo = (await behavior.GetStatusAsync([new NoImplicitInitialization()])).Response;
                        return result.ToResult([], behaviorInfo, HateoasLinks);
                    }
                    
                    return Results.NotFound();
                }
            );
            
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
        if (typeof(IStateMachineEndpoints).IsAssignableFrom(typeof(TStateMachine)))
        {
            var endpointsBuilder = new EndpointsBuilder(routeBuilder, this, interceptor, new StateMachineClass(stateMachineName));
            
            var stateMachine = (IStateMachineEndpoints)StateflowsActivator.CreateUninitializedInstance<TStateMachine>();
            stateMachine.RegisterEndpoints(endpointsBuilder);
        }
        
        return Task.CompletedTask;
    }

    public override Task VertexTypeAddedAsync<TVertex>(string stateMachineName, int stateMachineVersion, string vertexName)
    {
        if (typeof(IStateEndpoints).IsAssignableFrom(typeof(TVertex)))
        {
            var behaviorClass = new StateMachineClass(stateMachineName);
            
            DependencyInjection.StateMachineEndpointBuilders.Add(visitor =>
            {
                var builder = new EndpointsBuilder(routeBuilder, visitor, interceptor, behaviorClass, vertexName);
                
                var state = (IStateEndpoints)StateflowsActivator.CreateUninitializedInstance<TVertex>();
                state.RegisterEndpoints(builder);
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