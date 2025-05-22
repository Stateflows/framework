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
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.Extensions.MinimalAPIs;
   
public static class DependencyInjection
{
    internal static readonly List<System.Action<StateMachineVisitor>> StateMachineEndpointBuilders = [];
    
    internal static readonly List<System.Action<ActivityVisitor>> ActivityEndpointBuilders = [];
    
    internal static string ApiRoutePrefix = String.Empty;
    
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
            var builder = new EndpointsBuilder(visitor, stateMachineClass, stateName);
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
            var builder = new EndpointsBuilder(visitor, stateMachineClass);
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
            var builder = new EndpointsBuilder(visitor, activityClass);
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
            var builder = new EndpointsBuilder(visitor, activityClass, nodeName);
            endpointsBuilder(builder);
        });

        return (TReturn)structuredActivityBuilder;
    }

    /// <summary>
    /// Registers <a href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview">Minimal API</a>-based REST interface for Stateflows behaviors
    /// </summary>
    /// <param name="apiRoutePrefix">Prefix for all Stateflows endpoints</param>
    /// <param name="routeHandlerBuilderAction"></param>
    public static void MapStateflowsMinimalAPIsEndpoints(this IEndpointRouteBuilder builder, string apiRoutePrefix = "stateflows", System.Action<RouteHandlerBuilder>? routeHandlerBuilderAction = null)
    {
        ApiRoutePrefix = apiRoutePrefix;
        
        routeHandlerBuilderAction ??= _ => { };
        
        var root = string.IsNullOrEmpty(apiRoutePrefix)
            ? builder
            : builder.MapGroup($"/{apiRoutePrefix}");

        var behaviorClasses = root.MapGroup("/classes");
        
        var allClasses = behaviorClasses.MapGet(
            "/",
            (IBehaviorClassesProvider provider) => Results.Ok(provider.AllBehaviorClasses)
        );

        routeHandlerBuilderAction(allClasses);

        RegisterActionsApi(builder, routeHandlerBuilderAction, root, behaviorClasses);

        RegisterActivitiesApi(builder, routeHandlerBuilderAction, root, behaviorClasses);

        RegisterStateMachinesApi(builder, routeHandlerBuilderAction, root, behaviorClasses);
    }

    private static void RegisterActivitiesApi(IEndpointRouteBuilder builder, System.Action<RouteHandlerBuilder> routeHandlerBuilderAction,
        IEndpointRouteBuilder root, IEndpointRouteBuilder behaviorClasses)
    {
        var activitiesRegister = builder.ServiceProvider.GetService<IActivitiesRegister>();
        if (activitiesRegister != null)
        {
            var activities = root.MapGroup("/activities");

            routeHandlerBuilderAction(
                behaviorClasses.MapGet(
                    "/activities",
                    (IBehaviorClassesProvider provider) => Results.Ok(
                        provider.AllBehaviorClasses.Where(behaviorClass => behaviorClass.Type == ActivityClass.Type)
                    )
                )
            );

            routeHandlerBuilderAction(
                activities.MapGet(
                    "/",
                    async (IBehaviorClassesProvider provider, IStateflowsStorage storage) =>
                    {
                        var stateMachineClasses = provider.AllBehaviorClasses
                            .Where(c => c.Type == ActivityClass.Type)
                            .ToArray();
                        var contexts = await storage.GetAllContextsAsync(stateMachineClasses);
                        return Results.Ok(contexts.Select(context => context.Id));
                    }
                )
            );
            
            var visitor = new ActivityVisitor(
                activities,
                routeHandlerBuilderAction,
                builder.ServiceProvider.GetRequiredService<ITypeMapper>()
            );

            activitiesRegister.VisitActivitiesAsync(visitor);

            foreach (var action in ActivityEndpointBuilders)
            {
                action(visitor);
            }
        }
    }

    private static void RegisterActionsApi(IEndpointRouteBuilder builder, System.Action<RouteHandlerBuilder> routeHandlerBuilderAction,
        IEndpointRouteBuilder root, IEndpointRouteBuilder behaviorClasses)
    {
        var actionsRegister = builder.ServiceProvider.GetService<IActionsRegister>();
        if (actionsRegister != null)
        {
            var actions = root.MapGroup("/actions");
            
            routeHandlerBuilderAction(
                behaviorClasses.MapGet(
                    "/actions",
                    (IBehaviorClassesProvider provider) => Results.Ok(
                        provider.AllBehaviorClasses.Where(behaviorClass => behaviorClass.Type == ActionClass.Type)
                    )
                )
            );

            routeHandlerBuilderAction(
                actions.MapGet(
                    "/",
                    async (IBehaviorClassesProvider provider, IStateflowsStorage storage) =>
                    {
                        var stateMachineClasses = provider.AllBehaviorClasses
                            .Where(c => c.Type == ActionClass.Type)
                            .ToArray();
                        var contexts = await storage.GetAllContextsAsync(stateMachineClasses);
                        return Results.Ok(contexts.Select(context => context.Id));
                    }
                )
            );
            
            var visitor = new ActionVisitor(actions, routeHandlerBuilderAction);

            actionsRegister.VisitActionsAsync(visitor);
        }
    }

    private static void RegisterStateMachinesApi(IEndpointRouteBuilder builder, System.Action<RouteHandlerBuilder> routeHandlerBuilderAction,
        IEndpointRouteBuilder root, IEndpointRouteBuilder behaviorClasses)
    {
        var stateMachinesRegister = builder.ServiceProvider.GetService<IStateMachinesRegister>();
        if (stateMachinesRegister != null)
        {
            var stateMachines = root.MapGroup("/stateMachines");
            
            routeHandlerBuilderAction(
                behaviorClasses.MapGet(
                    "/stateMachines",
                    (IBehaviorClassesProvider provider) => Results.Ok(provider.AllBehaviorClasses.Where(behaviorClass => behaviorClass.Type == StateMachineClass.Type))
                )
            );

            routeHandlerBuilderAction(
                stateMachines.MapGet(
                    "/",
                    async (IBehaviorClassesProvider provider, IStateflowsStorage storage) =>
                    {
                        var stateMachineClasses = provider.AllBehaviorClasses
                            .Where(c => c.Type == StateMachineClass.Type)
                            .ToArray();
                        var contexts = await storage.GetAllContextsAsync(stateMachineClasses);
                        return Results.Ok(contexts.Select(context => context.Id));
                    }
                )
            );
            
            var visitor = new StateMachineVisitor(
                stateMachines,
                routeHandlerBuilderAction,
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