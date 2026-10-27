using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Attributes;

namespace Stateflows.Activities;

/// <summary>Provides activity registration helpers that integrate activity nodes with entity projections.</summary>
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
        public override async Task OnActivityInitializeAsync(IActivityInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
        {
            var result = await next();
            if (!result) return;

            var behaviorId = GetBehaviorId(context, scope);
            if (behaviorId.HasValue)
            {
                context.Behavior.SubscribeAsync<TProjectionTemplate>(GetEntityId(behaviorId.Value));
            }
        }
    }
    
    private static ActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this ActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => (ActivityBuilder)builder.AddObserver(c => new SubscriptionObserver<TProjectionTemplate>(scope));
    
    /// <summary>Subscribes an activity behavior to an entity projection during activity initialization.</summary>
    /// <typeparam name="TProjectionTemplate">The projection template to observe.</typeparam>
    /// <param name="builder">The activity builder.</param>
    /// <param name="scope">The behavior scope used to identify the entity instance.</param>
    /// <returns>The activity builder for further configuration.</returns>
    /// <remarks>For an activity, the subscription is attached during initialization; select <see cref="EntityScope"/> deliberately because the target may be the activity itself, its parent, or its owner.</remarks>
    public static IActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this IActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((ActivityBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    /// <summary>Adds an entity projection subscription to an inherited activity definition.</summary>
    /// <typeparam name="TProjectionTemplate">The projection template to observe.</typeparam>
    /// <param name="builder">The activity override builder.</param>
    /// <param name="scope">The behavior scope used to identify the entity instance.</param>
    /// <returns>The activity override builder for further configuration.</returns>
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
    
    /// <summary>Subscribes a structured activity to a projection for its active lifetime.</summary>
    /// <typeparam name="TProjectionTemplate">The projection template to observe.</typeparam>
    /// <param name="builder">The structured activity builder.</param>
    /// <param name="scope">The behavior scope used to identify the entity instance.</param>
    /// <returns>The structured activity builder for further configuration.</returns>
    /// <remarks>The helper subscribes on structured-node initialization and unsubscribes on finalization, so nested lifecycle scopes are paired.</remarks>
    public static IStructuredActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this IStructuredActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StructuredActivityBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    /// <summary>Adds a paired projection subscription to an inherited structured activity.</summary>
    /// <typeparam name="TProjectionTemplate">The projection template to observe.</typeparam>
    /// <param name="builder">The structured activity override builder.</param>
    /// <param name="scope">The behavior scope used to identify the entity instance.</param>
    /// <returns>The structured activity override builder for further configuration.</returns>
    public static IOverridenStructuredActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenStructuredActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StructuredActivityBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    /// <summary>Subscribes a reactive structured activity to a projection for its active lifetime.</summary>
    /// <typeparam name="TProjectionTemplate">The projection template to observe.</typeparam>
    /// <param name="builder">The reactive structured activity builder.</param>
    /// <param name="scope">The behavior scope used to identify the entity instance.</param>
    /// <returns>The reactive structured activity builder for further configuration.</returns>
    /// <remarks>The helper subscribes on structured-node initialization and unsubscribes on finalization, so nested lifecycle scopes are paired.</remarks>
    public static IReactiveStructuredActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this IReactiveStructuredActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StructuredActivityBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
    
    /// <summary>Adds a paired projection subscription to an inherited reactive structured activity.</summary>
    /// <typeparam name="TProjectionTemplate">The projection template to observe.</typeparam>
    /// <param name="builder">The reactive structured activity override builder.</param>
    /// <param name="scope">The behavior scope used to identify the entity instance.</param>
    /// <returns>The reactive structured activity override builder for further configuration.</returns>
    public static IOverridenReactiveStructuredActivityBuilder AddProjectionSubscription<TProjectionTemplate>(this IOverridenReactiveStructuredActivityBuilder builder, EntityScope scope = EntityScope.Self)
        => ((StructuredActivityBuilder)builder).AddProjectionSubscription<TProjectionTemplate>(scope);
}