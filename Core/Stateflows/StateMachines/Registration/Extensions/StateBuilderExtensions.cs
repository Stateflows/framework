using System;
using System.Threading.Tasks;
using Stateflows.Common.Attributes;
using Stateflows.Entities;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines;

/// <summary>Registers entity projection subscriptions bound to state activation or machine initialization.</summary>
/// <remarks>Entity identities combine the selected behavior's class and instance with the entity name.
/// State-level subscriptions are awaited on entry and removed on exit; machine-level subscriptions are queued
/// before each initialization attempt and are not paired with an exit callback.
/// Missing parent or owner contexts cause subscription callbacks to do nothing.
/// See <see href="https://github.com/Stateflows/framework/wiki/behavioral-groups">behavioral groups</see>.</remarks>
public static class StateBuilderExtensions
{
    private static EntityId GetEntityId(BehaviorId behaviorId, string entityName)
        => new EntityId($"{behaviorId.Name}.entity.{entityName}", behaviorId.Instance);

    private static BehaviorId? GetBehaviorId(IStateMachineActionContext context, EntityScope scope)
        => scope switch
        {
            EntityScope.Self => context.Behavior.Id,
            EntityScope.Parent => context.TryGetParentBehaviorContext(out var parentBehaviorContext)
                ? parentBehaviorContext.Id
                : null,
            EntityScope.Owner => context.TryGetOwnerBehaviorContext(out var ownerBehaviorContext)
                ? ownerBehaviorContext.Id
                : null,
            _ => null
        };

    private static TReturn AddProjectionSubscription<TReturn, TProjectionTemplate>(this IStateEvents<TReturn> builder,
        string entityName,
        EntityScope scope = EntityScope.Self)
        where TReturn : class
    {
        builder.AddOnEntry(c =>
        {
            var behaviorId = GetBehaviorId(c, scope);
            return behaviorId.HasValue
                ? c.Behavior.SubscribeAsync<TProjectionTemplate>(GetEntityId(behaviorId.Value, entityName))
                : Task.CompletedTask;
        });
        
        builder.AddOnExit(c =>
        {
            var behaviorId = GetBehaviorId(c, scope);
            return behaviorId.HasValue
                ? c.Behavior.UnsubscribeAsync<TProjectionTemplate>(GetEntityId(behaviorId.Value, entityName))
                : Task.CompletedTask;
        });

        return (TReturn)builder;
    }

    /// <summary>Subscribes on state entry and unsubscribes on state exit.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The state builder receiving the lifecycle callbacks.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original state builder.</returns>
    public static IStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on state entry and unsubscribes on state exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The state builder receiving the lifecycle callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original state builder.</returns>
    public static IStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on state entry and unsubscribes on state exit.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The behavior-state builder receiving the callbacks.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original behavior-state builder.</returns>
    public static IBehaviorStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IBehaviorStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IBehaviorStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on state entry and unsubscribes on state exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The behavior-state builder receiving the callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original behavior-state builder.</returns>
    public static IBehaviorStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IBehaviorStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IBehaviorStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on inherited-state entry and unsubscribes on exit.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited-state builder receiving the callbacks.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original inherited-state builder.</returns>
    public static IOverridenStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on inherited-state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited-state builder receiving the callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original inherited-state builder.</returns>
    public static IOverridenStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOverridenStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on state entry and unsubscribes on exit after regionalization.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The regionalized inherited-state builder receiving the callbacks.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original builder, preserving its regionalized stage.</returns>
    public static IOverridenRegionalizedStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenRegionalizedStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenRegionalizedStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The regionalized inherited-state builder receiving the callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original builder, preserving its regionalized stage.</returns>
    public static IOverridenRegionalizedStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenRegionalizedStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOverridenRegionalizedStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on inherited-state entry and unsubscribes on exit.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited behavior-state builder receiving the callbacks.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original inherited behavior-state builder.</returns>
    public static IBehaviorOverridenStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IBehaviorOverridenStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IBehaviorOverridenStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on inherited-state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited behavior-state builder receiving the callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original inherited behavior-state builder.</returns>
    public static IBehaviorOverridenStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IBehaviorOverridenStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IBehaviorOverridenStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on state entry and unsubscribes on exit after regionalization.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The regionalized inherited behavior-state builder.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original builder with its do-behavior and regionalization stages preserved.</returns>
    public static IBehaviorOverridenRegionalizedStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IBehaviorOverridenRegionalizedStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IBehaviorOverridenRegionalizedStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The regionalized inherited behavior-state builder.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original builder with its do-behavior and regionalization stages preserved.</returns>
    public static IBehaviorOverridenRegionalizedStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IBehaviorOverridenRegionalizedStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IBehaviorOverridenRegionalizedStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on composite-state entry and unsubscribes on exit.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The composite builder whose nested initial vertex is selected.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original initialized composite builder.</returns>
    public static IInitializedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IInitializedCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IInitializedCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on composite-state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The initialized composite builder receiving the callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original initialized composite builder.</returns>
    public static IInitializedCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IInitializedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IInitializedCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on composite-state entry and unsubscribes on exit.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The composite builder whose nested final state is defined.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original finalized composite-definition builder.</returns>
    public static IFinalizedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IFinalizedCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on composite-state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The finalized composite-definition builder receiving the callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original finalized composite-definition builder.</returns>
    public static IFinalizedCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IFinalizedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IFinalizedCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on composite-state entry and unsubscribes on exit.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The composite-state builder receiving the callbacks.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original composite-state builder.</returns>
    public static ICompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this ICompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<ICompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on composite-state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The composite-state builder receiving the callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original composite-state builder.</returns>
    public static ICompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this ICompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<ICompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on inherited composite-state entry and unsubscribes on exit.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited composite builder with a defined final state.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original finalized override builder.</returns>
    public static IFinalizedOverridenCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedOverridenCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IFinalizedOverridenCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on inherited composite-state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited composite builder with a defined final state.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original finalized override builder.</returns>
    public static IFinalizedOverridenCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IFinalizedOverridenCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)    
        where TTemplate : class
        => builder.AddProjectionSubscription<IFinalizedOverridenCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on composite-state entry and unsubscribes on exit after regionalization.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited composite builder with final-state and regionalization stages completed.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original builder, preserving its fluent definition stage.</returns>
    public static IFinalizedOverridenRegionalizedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedOverridenRegionalizedCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IFinalizedOverridenRegionalizedCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on composite-state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited composite builder with final-state and regionalization stages completed.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original builder, preserving its fluent definition stage.</returns>
    public static IFinalizedOverridenRegionalizedCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IFinalizedOverridenRegionalizedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IFinalizedOverridenRegionalizedCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on inherited composite-state entry and unsubscribes on exit.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited composite-state builder receiving the callbacks.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original inherited composite-state builder.</returns>
    public static IOverridenCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on inherited composite-state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited composite-state builder receiving the callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original inherited composite-state builder.</returns>
    public static IOverridenCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOverridenCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on composite-state entry and unsubscribes on exit after regionalization.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The regionalized inherited composite builder receiving the callbacks.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original regionalized composite builder.</returns>
    public static IOverridenRegionalizedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenRegionalizedCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenRegionalizedCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on composite-state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The regionalized inherited composite builder receiving the callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original regionalized composite builder.</returns>
    public static IOverridenRegionalizedCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenRegionalizedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOverridenRegionalizedCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on orthogonal-state entry and unsubscribes when the enclosing state exits.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The orthogonal-state builder receiving the callbacks.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original orthogonal-state builder.</returns>
    public static IOrthogonalStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOrthogonalStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOrthogonalStateBuilder, TProjectionTemplate>(entityName, scope);

    /// <summary>Subscribes on orthogonal-state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The orthogonal-state builder receiving the callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original orthogonal-state builder.</returns>
    public static IOrthogonalStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOrthogonalStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOrthogonalStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    /// <summary>Subscribes on inherited orthogonal-state entry and unsubscribes on exit using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited orthogonal-state builder receiving the callbacks.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original inherited orthogonal-state builder.</returns>
    public static IOverridenOrthogonalStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenOrthogonalStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOverridenOrthogonalStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    private class SubscriptionsObserver<TProjectionTemplate>(string entityName, EntityScope scope) : StateMachineObserver
    {
        public override async Task OnStateMachineInitializeAsync(IStateMachineInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
        {
            var behaviorId = GetBehaviorId(context, scope);
            if (behaviorId.HasValue)
            {
                _ = context.Behavior.SubscribeAsync<TProjectionTemplate>(GetEntityId(behaviorId.Value, entityName));
            }
            await next();
        }
    }
    
    private static StateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this StateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => (StateMachineBuilder)builder.AddObserver((_, _) => Task.FromResult<IStateMachineObserver>(new SubscriptionsObserver<TProjectionTemplate>(entityName, scope)));

    /// <summary>Queues a projection subscription before each machine initialization attempt.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The machine builder with its initial vertex selected.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original initialized definition builder.</returns>
    public static IInitializedStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IInitializedStateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(entityName, scope);
    
    /// <summary>Queues a projection subscription before each initialization attempt using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The machine builder with its initial vertex selected.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original initialized definition builder.</returns>
    public static IInitializedStateMachineBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IInitializedStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(Entity<TTemplate>.Name, scope);
    
    /// <summary>Queues a projection subscription before each machine initialization attempt.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The machine builder with its final state defined.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original finalized definition builder.</returns>
    public static IFinalizedStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedStateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(entityName, scope);
    
    /// <summary>Queues a projection subscription before each initialization attempt using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The machine builder with its final state defined.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original finalized definition builder.</returns>
    public static IFinalizedStateMachineBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IFinalizedStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(Entity<TTemplate>.Name, scope);
    
    /// <summary>Queues a projection subscription before each machine initialization attempt.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The machine builder receiving the subscription observer.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original machine builder.</returns>
    public static IStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IStateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(entityName, scope);
    
    /// <summary>Queues a projection subscription before each initialization attempt using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The machine builder receiving the subscription observer.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original machine builder.</returns>
    public static IStateMachineBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(Entity<TTemplate>.Name, scope);
    
    /// <summary>Queues a projection subscription before each inherited-machine initialization attempt.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited-machine builder receiving the subscription observer.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original inherited-machine builder.</returns>
    public static IOverridenStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenStateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(entityName, scope);
    
    /// <summary>Queues a projection subscription before each initialization attempt using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited-machine builder receiving the subscription observer.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original inherited-machine builder.</returns>
    public static IOverridenStateMachineBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(Entity<TTemplate>.Name, scope);
    
    /// <summary>Queues a projection subscription before each inherited-machine initialization attempt.</summary>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited-machine builder with its final state defined.</param>
    /// <param name="entityName">The entity name within the selected behavior scope.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original finalized override builder.</returns>
    public static IFinalizedOverridenStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedOverridenStateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(entityName, scope);
    
    /// <summary>Queues a projection subscription before each initialization attempt using a typed entity name.</summary>
    /// <typeparam name="TTemplate">The entity template providing its registered name.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection notification payload type.</typeparam>
    /// <param name="builder">The inherited-machine builder with its final state defined.</param>
    /// <param name="scope">The behavior identity used to address the entity.</param>
    /// <returns>The original finalized override builder.</returns>
    public static IFinalizedOverridenStateMachineBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IFinalizedOverridenStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(Entity<TTemplate>.Name, scope);
}