namespace Stateflows.StateMachines.Registration.Interfaces.Base;

/// <summary>Locates and extends event deferral rules inherited from a reused state definition.</summary>
/// <typeparam name="TReturn">The fluent state builder returned after extension.</typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.2.-In-State-Event-Deferrals" />
public interface IStateUtilsOverrides<out TReturn>
{
    /// <summary>Extends an inherited event-deferral rule on the current state.</summary>
    /// <typeparam name="TEvent">Deferred event payload type.</typeparam>
    /// <returns>The current state builder after extending the inherited deferral rule.</returns>
    /// <param name="buildAction">Optional callback configuring the inherited deferral guard. Without a guard, matching
    /// events are deferred unconditionally while the state is active.</param>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.2.-In-State-Event-Deferrals" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    TReturn UseDeferredEvent<TEvent>(OverridenDeferralBuildAction<TEvent> buildAction = null);
}
