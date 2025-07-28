using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.Extensions.MinimalAPIs;
   
public static class DependencyInjection
{
    internal static readonly List<System.Action<StateMachineVisitor>> StateMachineEndpointBuilders = [];
    
    internal static readonly List<System.Action<ActivityVisitor>> ActivityEndpointBuilders = [];
    
    internal static string ApiRoutePrefix = string.Empty;
    
    /// <summary>
    /// Allows to define <a href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview">Minimal API</a> endpoints in State Machine REST route group.
    /// <remarks>All endpoints defined here are <a href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters">filtered</a> to be available only when containing state is active.</remarks>
    /// </summary>
    /// <param name="endpointsBuilder">Endpoints builder</param>
    public static TReturn AddEndpoints<TReturn>(this IStateEvents<TReturn> stateBuilder,
        System.Action<IEndpointsBuilder> endpointsBuilder)
    {
        var stateMachineClass = ((IBehaviorBuilder)stateBuilder).BehaviorClass;
        var stateName = ((IVertexBuilder)stateBuilder).Name;
        StateMachineEndpointBuilders.Add(visitor =>
        {
            var builder = new EndpointsBuilder(visitor.RouteBuilder, visitor, visitor.Interceptor, stateMachineClass, stateName);
            endpointsBuilder(builder);
        });
        
        return (TReturn)stateBuilder;
    }

    /// <summary>
    /// Allows to define <a href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview">Minimal API</a> endpoints in State Machine REST route group.
    /// </summary>
    /// <param name="endpointsBuilder">Endpoints builder</param>
    public static TReturn AddEndpoints<TReturn>(this IStateMachineEvents<TReturn> stateMachineBuilder,
        System.Action<IEndpointsBuilder> endpointsBuilder)
    {
        var stateMachineClass = ((IBehaviorBuilder)stateMachineBuilder).BehaviorClass;
        StateMachineEndpointBuilders.Add(visitor =>
        {
            var builder = new EndpointsBuilder(visitor.RouteBuilder, visitor, visitor.Interceptor, stateMachineClass);
            endpointsBuilder(builder);
        });

        return (TReturn)stateMachineBuilder;
    }

    /// <summary>
    /// Allows to define <a href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview">Minimal API</a> endpoints in Activity REST route group.
    /// </summary>
    /// <param name="endpointsBuilder">Endpoints builder</param>
    public static TReturn AddEndpoints<TReturn>(this IActivityEvents<TReturn> activityBuilder,
        System.Action<IEndpointsBuilder> endpointsBuilder)
    {
        var activityClass = ((IBehaviorBuilder)activityBuilder).BehaviorClass;
        ActivityEndpointBuilders.Add(visitor =>
        {
            var builder = new EndpointsBuilder(visitor.RouteBuilder, visitor, visitor.Interceptor, activityClass);
            endpointsBuilder(builder);
        });

        return (TReturn)activityBuilder;
    }

    /// <summary>
    /// Allows to define <a href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview">Minimal API</a> endpoints in Activity REST route group.
    /// <remarks>All endpoints defined here are <a href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters">filtered</a> to be available only when containing node is active.</remarks>
    /// </summary>
    /// <param name="endpointsBuilder">Endpoints builder</param>
    public static TReturn AddEndpoints<TReturn>(this IStructuredActivityEvents<TReturn> structuredActivityBuilder,
        System.Action<IEndpointsBuilder> endpointsBuilder)
    {
        var activityClass = ((IBehaviorBuilder)structuredActivityBuilder).BehaviorClass;
        var nodeName = ((INodeBuilder)structuredActivityBuilder).Name;
        ActivityEndpointBuilders.Add(visitor =>
        {
            var builder = new EndpointsBuilder(visitor.RouteBuilder, visitor, visitor.Interceptor, activityClass, nodeName);
            endpointsBuilder(builder);
        });

        return (TReturn)structuredActivityBuilder;
    }

    /// <summary>
    /// Registers <a href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview">Minimal API</a>-based REST interface for Stateflows behaviors
    /// </summary>
    /// <param name="minimalAPIsBuilderAction">Configuration action</param>
    public static void MapStateflowsMinimalAPIsEndpoints(this IEndpointRouteBuilder builder, System.Action<IMinimalAPIsBuilder>? minimalAPIsBuilderAction = null)
    {
        ApiRoutePrefix = "stateflows";

        var interceptors = GetInterceptors(builder, minimalAPIsBuilderAction);

        var interceptor = new Interceptor(interceptors);
        
        var root = string.IsNullOrEmpty(ApiRoutePrefix)
            ? builder
            : builder.MapGroup($"/{ApiRoutePrefix}");

        var route = "/classes";
        var method = HttpMethods.Get;
        if (interceptor.BeforeGetAllClassesEndpointDefinition(ref method, ref route))
        {
            var allClasses = root.MapMethods(
                route,
                [method],
                (IBehaviorClassesProvider provider) =>
                {
                    var result = interceptor.FilterBehaviorClasses(provider.AllBehaviorClasses).ToArray();
                    return result.Length != 0
                        ? Results.Ok(result)
                        : Results.NotFound();
                });
            
            interceptor.AfterGetAllClassesEndpointDefinition(method, route, allClasses);
        }

        route = "/";
        if (interceptor.BeforeGetAllInstancesEndpointDefinition(ref method, ref route))
        {
            var endpointRouteBuilder = root.MapMethods(
                route,
                [method],
                async (IBehaviorClassesProvider provider, IStateflowsStorage storage) =>
                {
                    var behaviorClasses = provider.AllBehaviorClasses
                        .ToArray();
                    var contextIds = await storage.GetAllContextIdsAsync(behaviorClasses);
                    return Results.Ok(contextIds.Select(id => new { Id = id }));
                }
            );
            
            interceptor.AfterGetAllInstancesEndpointDefinition(method, route, endpointRouteBuilder);
        }

        RegisterActionsApi(builder, interceptor, root);

        RegisterActivitiesApi(builder, interceptor, root);

        RegisterStateMachinesApi(builder, interceptor, root);
    }

    private static IEnumerable<IEndpointDefinitionInterceptor> GetInterceptors(IEndpointRouteBuilder builder, System.Action<IMinimalAPIsBuilder>? minimalAPIsBuilderAction)
    {
        var minimalAPIsBuilder = new MinimalAPIsBuilder(builder.ServiceProvider);
        minimalAPIsBuilderAction?.Invoke(minimalAPIsBuilder);
        
        var actionsRegister = builder.ServiceProvider.GetService<IActionsRegister>();
        if (actionsRegister != null)
        {
            var visitor = new ActionConfigurationVisitor(minimalAPIsBuilder);
            actionsRegister.VisitActionsAsync(visitor);
        }
        
        var activitiesRegister = builder.ServiceProvider.GetService<IActivitiesRegister>();
        if (activitiesRegister != null)
        {
            var visitor = new ActivityConfigurationVisitor(minimalAPIsBuilder);
            activitiesRegister.VisitActivitiesAsync(visitor);
        }
        
        var stateMachinesRegister = builder.ServiceProvider.GetService<IStateMachinesRegister>();
        if (stateMachinesRegister != null)
        {
            var visitor = new StateMachineConfigurationVisitor(minimalAPIsBuilder);
            stateMachinesRegister.VisitStateMachinesAsync(visitor);
        }

        return minimalAPIsBuilder.GetInterceptors();
    }

    private static void RegisterActivitiesApi(IEndpointRouteBuilder builder, Interceptor interceptor,
        IEndpointRouteBuilder root)
    {
        var activitiesRegister = builder.ServiceProvider.GetService<IActivitiesRegister>();
        if (activitiesRegister != null)
        {
            var route = "/classes/activities";
            var method = HttpMethods.Get;
            if (interceptor.BeforeGetClassesEndpointDefinition(BehaviorType.Activity, ref method, ref route))
            {
                var endpointRouteBuilder = root.MapMethods(
                    route,
                    [method],
                    (IBehaviorClassesProvider provider) => Results.Ok(
                        provider.AllBehaviorClasses.Where(behaviorClass => behaviorClass.Type == ActivityClass.Type)
                    )
                );

                interceptor.AfterGetClassesEndpointDefinition(BehaviorType.Activity, method, route, endpointRouteBuilder);
            }

            route = "/activities";
            if (interceptor.BeforeGetInstancesEndpointDefinition(ActivityClass.Type, ref method, ref route))
            {
                var endpointRouteBuilder = root.MapMethods(
                    route,
                    [method],
                    async (IBehaviorClassesProvider provider, IStateflowsStorage storage) =>
                    {
                        var activityClasses = provider.AllBehaviorClasses
                            .Where(c => c.Type == ActivityClass.Type)
                            .ToArray();
                        var contextIds = await storage.GetAllContextIdsAsync(activityClasses);
                        return Results.Ok(contextIds.Select(id => new { Id = id }));
                    }
                );
            
                interceptor.AfterGetInstancesEndpointDefinition(ActivityClass.Type, method, route, endpointRouteBuilder);
            }

            var visitor = new ActivityVisitor(
                root,
                interceptor,
                builder.ServiceProvider.GetRequiredService<ITypeMapper>()
            );

            activitiesRegister.VisitActivitiesAsync(visitor);

            foreach (var action in ActivityEndpointBuilders)
            {
                action(visitor);
            }
        }
    }

    private static void RegisterActionsApi(IEndpointRouteBuilder builder, Interceptor interceptor,
        IEndpointRouteBuilder root)
    {
        var actionsRegister = builder.ServiceProvider.GetService<IActionsRegister>();
        if (actionsRegister != null)
        {
            var route = "/classes/actions";
            var method = HttpMethods.Get;
            if (interceptor.BeforeGetClassesEndpointDefinition(BehaviorType.Action, ref method, ref route))
            {
                var endpointRouteBuilder = root.MapMethods(
                    route,
                    [method],
                    (IBehaviorClassesProvider provider) => Results.Ok(
                        provider.AllBehaviorClasses.Where(behaviorClass => behaviorClass.Type == ActionClass.Type)
                    )
                );

                interceptor.AfterGetClassesEndpointDefinition(BehaviorType.Action, method, route, endpointRouteBuilder);
            }

            route = "/actions";
            if (interceptor.BeforeGetInstancesEndpointDefinition(ActionClass.Type, ref method, ref route))
            {
                var endpointRouteBuilder = root.MapMethods(
                    route,
                    [method],
                    async (IBehaviorClassesProvider provider, IStateflowsStorage storage) =>
                    {
                        var actionClasses = provider.AllBehaviorClasses
                            .Where(c => c.Type == ActionClass.Type)
                            .ToArray();
                        var contextIds = await storage.GetAllContextIdsAsync(actionClasses);
                        return Results.Ok(contextIds.Select(id => new { Id = id }));
                    }
                );
            
                interceptor.AfterGetInstancesEndpointDefinition(ActionClass.Type, method, route, endpointRouteBuilder);
            }

            var visitor = new ActionVisitor(root, interceptor);

            actionsRegister.VisitActionsAsync(visitor);
        }
    }

    private static void RegisterStateMachinesApi(IEndpointRouteBuilder builder, Interceptor interceptor,
        IEndpointRouteBuilder root)
    {
        var stateMachinesRegister = builder.ServiceProvider.GetService<IStateMachinesRegister>();
        if (stateMachinesRegister != null)
        {
            var route = "/classes/stateMachines";
            var method = HttpMethods.Get;
            if (interceptor.BeforeGetClassesEndpointDefinition(BehaviorType.StateMachine, ref method, ref route))
            {
                var endpointRouteBuilder = root.MapMethods(
                    route,
                    [method],
                    (IBehaviorClassesProvider provider) => Results.Ok(provider.AllBehaviorClasses.Where(behaviorClass => behaviorClass.Type == StateMachineClass.Type))
                );

                interceptor.AfterGetClassesEndpointDefinition(BehaviorType.StateMachine, method, route, endpointRouteBuilder);
            }

            route = "/stateMachines";
            if (interceptor.BeforeGetInstancesEndpointDefinition(StateMachineClass.Type, ref method, ref route))
            {
                var endpointRouteBuilder = root.MapMethods(
                    route,
                    [method],
                    async (IBehaviorClassesProvider provider, IStateflowsStorage storage) =>
                    {
                        var actionClasses = provider.AllBehaviorClasses
                            .Where(c => c.Type == StateMachineClass.Type)
                            .ToArray();
                        var contextIds = await storage.GetAllContextIdsAsync(actionClasses);
                        return Results.Ok(contextIds.Select(id => new { Id = id }));
                    }
                );
            
                interceptor.AfterGetInstancesEndpointDefinition(StateMachineClass.Type, method, route, endpointRouteBuilder);
            }
            
            var visitor = new StateMachineVisitor(
                root,
                interceptor,
                builder.ServiceProvider.GetRequiredService<ITypeMapper>()
            );
            
            stateMachinesRegister.VisitStateMachinesAsync(visitor);

            foreach (var action in StateMachineEndpointBuilders)
            {
                action(visitor);
            }
        }
    }
}