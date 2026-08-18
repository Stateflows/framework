using System.Threading.Tasks;
using Stateflows.Common.Attributes;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines;

public static class StateBuilderExtensions
{
    private static EntityId GetEntityId(BehaviorId behaviorId)
        => new EntityId($"{behaviorId.Name}.entity", behaviorId.Instance);

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
        EntityScope scope = EntityScope.Self)
        where TReturn : class
    {
        builder.AddOnEntry(c =>
        {
            var behaviorId = GetBehaviorId(c, scope);
            return behaviorId.HasValue
                ? c.Behavior.SubscribeAsync<TProjectionTemplate>(GetEntityId(behaviorId.Value))
                : Task.CompletedTask;
        });
        
        builder.AddOnExit(c =>
        {
            var behaviorId = GetBehaviorId(c, scope);
            return behaviorId.HasValue
                ? c.Behavior.UnsubscribeAsync<TProjectionTemplate>(GetEntityId(behaviorId.Value))
                : Task.CompletedTask;
        });

        return (TReturn)builder;
    }

    public static IStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IStateBuilder, TProjectionTemplate>(scope);
    
    public static IBehaviorStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IBehaviorStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IBehaviorStateBuilder, TProjectionTemplate>(scope);
    
    public static IOverridenStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenStateBuilder, TProjectionTemplate>(scope);
    
    public static IOverridenRegionalizedStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenRegionalizedStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenRegionalizedStateBuilder, TProjectionTemplate>(scope);
    
    public static IBehaviorOverridenStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IBehaviorOverridenStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IBehaviorOverridenStateBuilder, TProjectionTemplate>(scope);
    
    public static IBehaviorOverridenRegionalizedStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IBehaviorOverridenRegionalizedStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IBehaviorOverridenRegionalizedStateBuilder, TProjectionTemplate>(scope);
    
    public static IInitializedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IInitializedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IInitializedCompositeStateBuilder, TProjectionTemplate>(scope);
        
    public static IFinalizedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IFinalizedCompositeStateBuilder, TProjectionTemplate>(scope);
        
    public static ICompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this ICompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<ICompositeStateBuilder, TProjectionTemplate>(scope);
        
    public static IFinalizedOverridenCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedOverridenCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IFinalizedOverridenCompositeStateBuilder, TProjectionTemplate>(scope);
        
    public static IFinalizedOverridenRegionalizedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedOverridenRegionalizedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IFinalizedOverridenRegionalizedCompositeStateBuilder, TProjectionTemplate>(scope);
        
    public static IOverridenCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenCompositeStateBuilder, TProjectionTemplate>(scope);
        
    public static IOverridenRegionalizedCompositeStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenRegionalizedCompositeStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenRegionalizedCompositeStateBuilder, TProjectionTemplate>(scope);
    
    public static IOrthogonalStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOrthogonalStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOrthogonalStateBuilder, TProjectionTemplate>(scope);
    
    public static IOverridenOrthogonalStateBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenOrthogonalStateBuilder builder, EntityScope scope = EntityScope.Self)
        => builder.AddProjectionSubscription<IOverridenOrthogonalStateBuilder, TProjectionTemplate>(scope);

    private class SubscriptionsObserver<TProjectionTemplate>(EntityScope scope) : StateMachineObserver
    {
        public override void AfterStateMachineInitialize(IStateMachineInitializationContext context, bool implicitInitialization, bool initialized)
        {
            if (!initialized) return;
            
            var behaviorId = GetBehaviorId(context, scope);
            if (behaviorId.HasValue)
            {
                context.Behavior.SubscribeAsync<TProjectionTemplate>(GetEntityId(behaviorId.Value));
            }
        }
    }
    
    private static StateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this StateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        => (StateMachineBuilder)builder.AddObserver((_, _) => Task.FromResult<IStateMachineObserver>(new SubscriptionsObserver<TProjectionTemplate>(scope)));

    public static IInitializedStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IInitializedStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IFinalizedStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IOverridenStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IFinalizedOverridenStateMachineBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedOverridenStateMachineBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IInitializedStateMachineWithEntityBuilder AddProjectionSubscription<TProjectionTemplate>(this IInitializedStateMachineWithEntityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IFinalizedStateMachineWithEntityBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedStateMachineWithEntityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IStateMachineWithEntityBuilder AddProjectionSubscription<TProjectionTemplate>(this IStateMachineWithEntityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IOverridenStateMachineWithEntityBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenStateMachineWithEntityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IFinalizedOverridenStateMachineWithEntityBuilder AddProjectionSubscription<TProjectionTemplate>(this IFinalizedOverridenStateMachineWithEntityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StateMachineBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
}