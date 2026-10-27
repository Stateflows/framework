using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration;
using Stateflows.Entities;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.Extensions.MinimalAPIs;
   
/// <summary>Registers custom behavior endpoints and maps Stateflows' generated HTTP API.</summary>
/// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/extension-minimal-apis"/>.</remarks>
public static class DependencyInjection
{
    internal static readonly List<System.Action<StateMachineVisitor>> StateMachineEndpointBuilders = [];
    
    internal static readonly List<System.Action<ActivityVisitor>> ActivityEndpointBuilders = [];
    
    internal static string ApiRoutePrefix = string.Empty;
    
    internal static Interceptor Interceptor { get; private set; }
    
    /// <summary>
    /// Registers custom Minimal API endpoints scoped to a State Machine state.
    /// </summary>
    /// <remarks>Endpoints are available only while the containing state is active.</remarks>
    /// <typeparam name="TReturn">Fluent state builder returned after registration.</typeparam>
    /// <param name="stateBuilder">State whose active configuration controls endpoint availability.</param>
    /// <param name="endpointsBuilder">Callback defining the custom routes and handlers.</param>
    /// <returns>The original state builder.</returns>
    public static TReturn AddEndpoints<TReturn>(this IStateEvents<TReturn> stateBuilder,
        System.Action<IEndpointsBuilder> endpointsBuilder)
    {
        var stateMachineClass = ((IBehaviorBuilder)stateBuilder).BehaviorClass;
        var stateName = ((IStateBuilderInfo)stateBuilder).Name;
        StateMachineEndpointBuilders.Add(visitor =>
        {
            var builder = new EndpointsBuilder(visitor.RouteBuilder, visitor, visitor.Interceptor, stateMachineClass, visitor.HasDefaultInstance, stateName);
            endpointsBuilder(builder);
        });
        
        return (TReturn)stateBuilder;
    }

    /// <summary>
    /// Registers custom Minimal API endpoints for a State Machine class.
    /// </summary>
    /// <typeparam name="TReturn">Fluent State Machine builder returned after registration.</typeparam>
    /// <param name="stateMachineBuilder">Definition receiving the endpoints.</param>
    /// <param name="endpointsBuilder">Callback defining the custom routes and handlers.</param>
    /// <returns>The original State Machine builder.</returns>
    public static TReturn AddEndpoints<TReturn>(this IStateMachineEvents<TReturn> stateMachineBuilder,
        System.Action<IEndpointsBuilder> endpointsBuilder)
    {
        var stateMachineClass = ((IBehaviorBuilder)stateMachineBuilder).BehaviorClass;
        StateMachineEndpointBuilders.Add(visitor =>
        {
            var builder = new EndpointsBuilder(visitor.RouteBuilder, visitor, visitor.Interceptor, stateMachineClass, visitor.HasDefaultInstance);
            endpointsBuilder(builder);
        });

        return (TReturn)stateMachineBuilder;
    }

    /// <summary>
    /// Registers custom Minimal API endpoints for an Activity class.
    /// </summary>
    /// <typeparam name="TReturn">Fluent Activity builder returned after registration.</typeparam>
    /// <param name="activityBuilder">Definition receiving the endpoints.</param>
    /// <param name="endpointsBuilder">Callback defining the custom routes and handlers.</param>
    /// <returns>The original Activity builder.</returns>
    public static TReturn AddEndpoints<TReturn>(this IActivityEvents<TReturn> activityBuilder,
        System.Action<IEndpointsBuilder> endpointsBuilder)
    {
        var activityClass = ((IBehaviorBuilder)activityBuilder).BehaviorClass;
        ActivityEndpointBuilders.Add(visitor =>
        {
            var builder = new EndpointsBuilder(visitor.RouteBuilder, visitor, visitor.Interceptor, activityClass, visitor.HasDefaultInstance);
            endpointsBuilder(builder);
        });

        return (TReturn)activityBuilder;
    }

    /// <summary>
    /// Registers custom Minimal API endpoints scoped to a structured Activity node.
    /// </summary>
    /// <remarks>Endpoints are available only while the containing node is active.</remarks>
    /// <typeparam name="TReturn">Fluent structured-node builder returned after registration.</typeparam>
    /// <param name="structuredActivityBuilder">Node whose active configuration controls endpoint availability.</param>
    /// <param name="endpointsBuilder">Callback defining the custom routes and handlers.</param>
    /// <returns>The original structured-node builder.</returns>
    public static TReturn AddEndpoints<TReturn>(this IStructuredActivityEvents<TReturn> structuredActivityBuilder,
        System.Action<IEndpointsBuilder> endpointsBuilder)
    {
        var activityClass = ((IBehaviorBuilder)structuredActivityBuilder).BehaviorClass;
        var nodeName = ((INodeBuilder)structuredActivityBuilder).Name;
        ActivityEndpointBuilders.Add(visitor =>
        {
            var builder = new EndpointsBuilder(visitor.RouteBuilder, visitor, visitor.Interceptor, activityClass, visitor.HasDefaultInstance, nodeName);
            endpointsBuilder(builder);
        });

        return (TReturn)structuredActivityBuilder;
    }

    /// <summary>
    /// Maps discovery, event, and custom endpoints for registered Stateflows behavior definitions.
    /// </summary>
    /// <param name="builder">Application or route group on which to map the API.</param>
    /// <param name="minimalAPIsBuilderAction">Optional callback configuring route prefixes, endpoint rules, interceptors, and validation schemas.</param>
    /// <remarks>
    /// The default prefix is <c>/stateflows</c>. Behavior definitions must be registered before mapping:
    /// mapping visits the registered definitions to discover events and custom endpoints.
    /// Endpoint conventions, including authorization, must be configured for the application's needs.
    /// </remarks>
    public static void MapStateflowsMinimalAPIsEndpoints(this IEndpointRouteBuilder builder, System.Action<IMinimalAPIsBuilder>? minimalAPIsBuilderAction = null)
    {
        ApiRoutePrefix = "stateflows";

        var interceptors = GetInterceptors(builder, minimalAPIsBuilderAction, out var minimalAPIsBuilder);

        Interceptor = new Interceptor(interceptors)
        {
            ValidationSchemaMappers = new Dictionary<Type, System.Action<System.ComponentModel.DataAnnotations.ValidationAttribute, Microsoft.OpenApi.OpenApiSchema>>(minimalAPIsBuilder.ValidationSchemaMappers)
        };
        
        var root = string.IsNullOrEmpty(ApiRoutePrefix)
            ? builder
            : builder.MapGroup($"/{ApiRoutePrefix}");

        var route = "/classes";
        var method = HttpMethods.Get;
        if (Interceptor.BeforeGetAllClassesEndpointDefinition(ref method, ref route))
        {
            var allClasses = root.MapMethods(
                route,
                [method],
                (IBehaviorClassesProvider provider) =>
                {
                    var result = Interceptor.FilterBehaviorClasses(provider.AllBehaviorClasses).ToArray();
                    return result.Length != 0
                        ? Results.Ok(result)
                        : Results.NotFound();
                }
            )
            .WithTags(nameof(Stateflows));
            
            Interceptor.AfterGetAllClassesEndpointDefinition(method, route, allClasses);
        }

        route = "/";
        if (Interceptor.BeforeGetAllInstancesEndpointDefinition(ref method, ref route))
        {
            var endpointRouteBuilder = root.MapMethods(
                route,
                [method],
                async (IBehaviorClassesProvider provider, IStateflowsStorage storage, ITenantAccessor tenantAccessor) =>
                {
                    tenantAccessor.CurrentTenantId ??= "host";
                    var behaviorClasses = Interceptor.FilterBehaviorClasses(provider.AllBehaviorClasses)
                        .ToArray();
                    var contextIds = await storage.GetAllContextIdsAsync(behaviorClasses);
                    return Results.Ok(contextIds.Select(id => new { Id = id }));
                }
            )
            .WithTags(nameof(Stateflows));
            
            Interceptor.AfterGetAllInstancesEndpointDefinition(method, route, endpointRouteBuilder);
        }

        RegisterActionsApi(builder, Interceptor, root);

        RegisterActivitiesApi(builder, Interceptor, root);

        RegisterStateMachinesApi(builder, Interceptor, root);

        RegisterEntitiesApi(builder, Interceptor, root);
    }

    private static IEnumerable<IEndpointDefinitionInterceptor> GetInterceptors(IEndpointRouteBuilder builder, System.Action<IMinimalAPIsBuilder>? minimalAPIsBuilderAction, out MinimalAPIsBuilder minimalAPIsBuilder)
    {
        minimalAPIsBuilder = new MinimalAPIsBuilder(builder.ServiceProvider);
        minimalAPIsBuilderAction?.Invoke(minimalAPIsBuilder);
        
        var actionsRegister = builder.ServiceProvider.GetService<IActionsRegister>();
        if (actionsRegister != null)
        {
            var visitor = new ActionConfigurationVisitor(minimalAPIsBuilder);
            actionsRegister.VisitActionsAsync(visitor).GetAwaiter().GetResult();
        }
        
        var activitiesRegister = builder.ServiceProvider.GetService<IActivitiesRegister>();
        if (activitiesRegister != null)
        {
            var visitor = new ActivityConfigurationVisitor(minimalAPIsBuilder);
            activitiesRegister.VisitActivitiesAsync(visitor).GetAwaiter().GetResult();
        }
        
        var stateMachinesRegister = builder.ServiceProvider.GetService<IStateMachinesRegister>();
        if (stateMachinesRegister != null)
        {
            var visitor = new StateMachineConfigurationVisitor(minimalAPIsBuilder);
            stateMachinesRegister.VisitStateMachinesAsync(visitor).GetAwaiter().GetResult();
        }

        var entitiesRegister = builder.ServiceProvider.GetService<IEntitiesRegister>();
        if (entitiesRegister != null)
        {
            var visitor = new EntityConfigurationVisitor(minimalAPIsBuilder);
            entitiesRegister.VisitEntitiesAsync(visitor).GetAwaiter().GetResult();
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
                    (IBehaviorClassesProvider provider) =>
                    {
                        var result = interceptor.FilterBehaviorClasses(provider.AllBehaviorClasses.Where(behaviorClass =>
                            behaviorClass.Type == ActivityClass.Type)).ToArray();
                        return result.Length != 0
                            ? Results.Ok(result)
                            : Results.NotFound();
                    }
                )
                .WithTags(nameof(Stateflows));

                interceptor.AfterGetClassesEndpointDefinition(BehaviorType.Activity, method, route, endpointRouteBuilder);
            }

            route = "/activities";
            if (interceptor.BeforeGetInstancesEndpointDefinition(ActivityClass.Type, ref method, ref route))
            {
                var endpointRouteBuilder = root.MapMethods(
                    route,
                    [method],
                    async (IBehaviorClassesProvider provider, IStateflowsStorage storage, ITenantAccessor tenantAccessor) =>
                    {
                        tenantAccessor.CurrentTenantId ??= "host";
                        var activityClasses = interceptor.FilterBehaviorClasses(provider.AllBehaviorClasses
                            .Where(c => c.Type == ActivityClass.Type))
                            .ToArray();
                        var contextIds = await storage.GetAllContextIdsAsync(activityClasses);
                        return Results.Ok(contextIds.Select(id => new { Id = id }));
                    }
                )
                .WithTags(nameof(Stateflows));
            
                interceptor.AfterGetInstancesEndpointDefinition(ActivityClass.Type, method, route, endpointRouteBuilder);
            }

            var visitor = new ActivityVisitor(
                root,
                interceptor,
                builder.ServiceProvider.GetRequiredService<ITypeMapper>()
            );

            activitiesRegister.VisitActivitiesAsync(visitor).GetAwaiter().GetResult();

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
                    (IBehaviorClassesProvider provider) =>
                    {
                        var result = interceptor.FilterBehaviorClasses(provider.AllBehaviorClasses.Where(behaviorClass =>
                            behaviorClass.Type == ActionClass.Type)).ToArray();
                        return result.Length != 0
                            ? Results.Ok(result)
                            : Results.NotFound();
                    }
                )
                .WithTags(nameof(Stateflows));

                interceptor.AfterGetClassesEndpointDefinition(BehaviorType.Action, method, route, endpointRouteBuilder);
            }

            route = "/actions";
            if (interceptor.BeforeGetInstancesEndpointDefinition(ActionClass.Type, ref method, ref route))
            {
                var endpointRouteBuilder = root.MapMethods(
                    route,
                    [method],
                    async (IBehaviorClassesProvider provider, IStateflowsStorage storage, ITenantAccessor tenantAccessor) =>
                    {
                        tenantAccessor.CurrentTenantId ??= "host";
                        var actionClasses = interceptor.FilterBehaviorClasses(provider.AllBehaviorClasses
                            .Where(c => c.Type == ActionClass.Type))
                            .ToArray();
                        var contextIds = await storage.GetAllContextIdsAsync(actionClasses);
                        return Results.Ok(contextIds.Select(id => new { Id = id }));
                    }
                )
                .WithTags(nameof(Stateflows));
            
                interceptor.AfterGetInstancesEndpointDefinition(ActionClass.Type, method, route, endpointRouteBuilder);
            }

            var visitor = new ActionVisitor(root, interceptor, builder.ServiceProvider.GetRequiredService<ITypeMapper>());

            actionsRegister.VisitActionsAsync(visitor).GetAwaiter().GetResult();
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
                    (IBehaviorClassesProvider provider) =>
                    {
                        var result = interceptor.FilterBehaviorClasses(provider.AllBehaviorClasses.Where(behaviorClass =>
                            behaviorClass.Type == StateMachineClass.Type)).ToArray();
                        return result.Length != 0
                            ? Results.Ok(result)
                            : Results.NotFound();
                    }
                )
                .WithTags(nameof(Stateflows));

                interceptor.AfterGetClassesEndpointDefinition(BehaviorType.StateMachine, method, route, endpointRouteBuilder);
            }

            route = "/stateMachines";
            if (interceptor.BeforeGetInstancesEndpointDefinition(StateMachineClass.Type, ref method, ref route))
            {
                var endpointRouteBuilder = root.MapMethods(
                    route,
                    [method],
                    async (IBehaviorClassesProvider provider, IStateflowsStorage storage, ITenantAccessor tenantAccessor) =>
                    {
                        tenantAccessor.CurrentTenantId ??= "host";
                        var actionClasses = interceptor.FilterBehaviorClasses(provider.AllBehaviorClasses
                            .Where(c => c.Type == StateMachineClass.Type))
                            .ToArray();
                        var contextIds = await storage.GetAllContextIdsAsync(actionClasses);
                        return Results.Ok(contextIds.Select(id => new { Id = id }));
                    }
                )
                .WithTags(nameof(Stateflows));
            
                interceptor.AfterGetInstancesEndpointDefinition(StateMachineClass.Type, method, route, endpointRouteBuilder);
            }
            
            var visitor = new StateMachineVisitor(
                root,
                interceptor,
                builder.ServiceProvider.GetRequiredService<ITypeMapper>()
            );
            
            stateMachinesRegister.VisitStateMachinesAsync(visitor).GetAwaiter().GetResult();

            foreach (var action in StateMachineEndpointBuilders)
            {
                action(visitor);
            }
        }
    }

    private static void RegisterEntitiesApi(IEndpointRouteBuilder builder, Interceptor interceptor,
        IEndpointRouteBuilder root)
    {
        var entitiesRegister = builder.ServiceProvider.GetService<IEntitiesRegister>();
        if (entitiesRegister == null)
        {
            return;
        }

        var route = "/classes/entities";
        var method = HttpMethods.Get;
        if (interceptor.BeforeGetClassesEndpointDefinition(BehaviorType.Entity, ref method, ref route))
        {
            var endpointRouteBuilder = root.MapMethods(
                    route,
                    [method],
                    (IBehaviorClassesProvider provider) =>
                    {
                        var result = interceptor.FilterBehaviorClasses(provider.AllBehaviorClasses.Where(behaviorClass =>
                            behaviorClass.Type == EntityClass.Type)).ToArray();
                        return result.Length != 0
                            ? Results.Ok(result)
                            : Results.NotFound();
                    }
                )
                .WithTags(nameof(Stateflows));

            interceptor.AfterGetClassesEndpointDefinition(BehaviorType.Entity, method, route, endpointRouteBuilder);
        }

        var instancesRoute = "/entities";
        var instancesMethod = HttpMethods.Get;
        if (interceptor.BeforeGetInstancesEndpointDefinition(EntityClass.Type, ref instancesMethod, ref instancesRoute))
        {
            var endpointRouteBuilder = root.MapMethods(
                    instancesRoute,
                    [instancesMethod],
                    async (IBehaviorClassesProvider provider, IStateflowsStorage storage, ITenantAccessor tenantAccessor) =>
                    {
                        tenantAccessor.CurrentTenantId ??= "host";
                        var entityClasses = interceptor.FilterBehaviorClasses(provider.AllBehaviorClasses
                                .Where(c => c.Type == EntityClass.Type))
                            .ToArray();
                        var contextIds = await storage.GetAllContextIdsAsync(entityClasses);
                        return Results.Ok(contextIds.Select(id => new { Id = id }));
                    }
                )
                .WithTags(nameof(Stateflows));

            interceptor.AfterGetInstancesEndpointDefinition(EntityClass.Type, instancesMethod, instancesRoute, endpointRouteBuilder);
        }

        var visitor = new EntityVisitor(root, interceptor);
        entitiesRegister.VisitEntitiesAsync(visitor).GetAwaiter().GetResult();
        visitor.RegisterEndpoints();
    }
}