using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Attributes;

namespace Stateflows.Activities;

public static class ActivityBuilderExtensions
{
    private static EntityId GetEntityId(BehaviorId behaviorId)
        => new EntityId($"{behaviorId.Name}.entity", behaviorId.Instance);

    private static BehaviorId? GetBehaviorId(IActivityActionContext context, EntityScope scope)
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
    
    private class SubscriptionObserver<TProjectionTemplate>(EntityScope scope) : ActivityObserver
    {
        public override void AfterActivityInitialize(IActivityInitializationContext context, bool implicitInitialization, bool initialized)
        {
            if (!initialized) return;
            
            var behaviorId = GetBehaviorId(context, scope);
            if (behaviorId.HasValue)
            {
                context.Behavior.SubscribeAsync<TProjectionTemplate>(GetEntityId(behaviorId.Value));
            }
        }
    }
    
    private static ActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this ActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => (ActivityBuilder)builder.AddObserver(c => new SubscriptionObserver<TProjectionTemplate>(scope));
    
    public static IActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this IActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((ActivityBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IOverridenActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((ActivityBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);

    private static StructuredActivityBuilder AddProjectionSubscription<TProjectionTemplate>(
        this StructuredActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => (StructuredActivityBuilder)builder
            .AddOnInitialize(c =>
            {
                var behaviorId = GetBehaviorId(c, scope);
                return behaviorId.HasValue
                    ? c.Behavior.SubscribeAsync<TProjectionTemplate>(GetEntityId(behaviorId.Value))
                    : Task.CompletedTask;
            })
            .AddOnFinalize(c =>
            {
                var behaviorId = GetBehaviorId(c, scope);
                return behaviorId.HasValue
                    ? c.Behavior.UnsubscribeAsync<TProjectionTemplate>(GetEntityId(behaviorId.Value))
                    : Task.CompletedTask;
            });
    
    public static IStructuredActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this IStructuredActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StructuredActivityBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IOverridenStructuredActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenStructuredActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StructuredActivityBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IReactiveStructuredActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this IReactiveStructuredActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StructuredActivityBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    public static IOverridenReactiveStructuredActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenReactiveStructuredActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StructuredActivityBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
}