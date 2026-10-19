using System.Threading.Tasks;
using Stateflows.Common.Attributes;
using Stateflows.Entities;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines;

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

    public static IStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IBehaviorStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IBehaviorStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IBehaviorStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IBehaviorStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IBehaviorStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IBehaviorStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IOverridenStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IOverridenStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOverridenStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IOverridenRegionalizedStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenRegionalizedStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenRegionalizedStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IOverridenRegionalizedStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenRegionalizedStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOverridenRegionalizedStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IBehaviorOverridenStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IBehaviorOverridenStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IBehaviorOverridenStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IBehaviorOverridenStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IBehaviorOverridenStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IBehaviorOverridenStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IBehaviorOverridenRegionalizedStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IBehaviorOverridenRegionalizedStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IBehaviorOverridenRegionalizedStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IBehaviorOverridenRegionalizedStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IBehaviorOverridenRegionalizedStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IBehaviorOverridenRegionalizedStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IInitializedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IInitializedCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IInitializedCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IInitializedCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IInitializedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IInitializedCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IFinalizedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IFinalizedCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IFinalizedCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IFinalizedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IFinalizedCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static ICompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this ICompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<ICompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    public static ICompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this ICompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<ICompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IFinalizedOverridenCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedOverridenCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IFinalizedOverridenCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IFinalizedOverridenCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IFinalizedOverridenCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)    
        where TTemplate : class
        => builder.AddProjectionSubscription<IFinalizedOverridenCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IFinalizedOverridenRegionalizedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedOverridenRegionalizedCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IFinalizedOverridenRegionalizedCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IFinalizedOverridenRegionalizedCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IFinalizedOverridenRegionalizedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IFinalizedOverridenRegionalizedCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IOverridenCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IOverridenCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOverridenCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IOverridenRegionalizedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenRegionalizedCompositeStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenRegionalizedCompositeStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IOverridenRegionalizedCompositeStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenRegionalizedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOverridenRegionalizedCompositeStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IOrthogonalStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOrthogonalStateBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOrthogonalStateBuilder, TProjectionTemplate>(entityName, scope);

    public static IOrthogonalStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOrthogonalStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOrthogonalStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    public static IOverridenOrthogonalStateBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenOrthogonalStateBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => builder.AddProjectionSubscription<IOverridenOrthogonalStateBuilder, TProjectionTemplate>(Entity<TTemplate>.Name, scope);

    private class SubscriptionsObserver<TProjectionTemplate>(string entityName, EntityScope scope) : StateMachineObserver
    {
        public override void BeforeStateMachineInitialize(IStateMachineInitializationContext context, bool implicitInitialization)
        {
            var behaviorId = GetBehaviorId(context, scope);
            if (behaviorId.HasValue)
            {
                _ = context.Behavior.SubscribeAsync<TProjectionTemplate>(GetEntityId(behaviorId.Value, entityName));
            }
        }
    }
    
    private static StateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this StateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => (StateMachineBuilder)builder.AddObserver((_, _) => Task.FromResult<IStateMachineObserver>(new SubscriptionsObserver<TProjectionTemplate>(entityName, scope)));

    public static IInitializedStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IInitializedStateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(entityName, scope);
    
    public static IInitializedStateMachineBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IInitializedStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(Entity<TTemplate>.Name, scope);
    
    public static IFinalizedStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedStateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(entityName, scope);
    
    public static IFinalizedStateMachineBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IFinalizedStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(Entity<TTemplate>.Name, scope);
    
    public static IStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IStateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(entityName, scope);
    
    public static IStateMachineBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(Entity<TTemplate>.Name, scope);
    
    public static IOverridenStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenStateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(entityName, scope);
    
    public static IOverridenStateMachineBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IOverridenStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(Entity<TTemplate>.Name, scope);
    
    public static IFinalizedOverridenStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedOverridenStateMachineBuilder builder, string entityName, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(entityName, scope);
    
    public static IFinalizedOverridenStateMachineBuilder AddProjectionSubscription<TTemplate, TProjectionTemplate>(this IFinalizedOverridenStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        where TTemplate : class
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(Entity<TTemplate>.Name, scope);
}