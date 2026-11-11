using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    /// <summary>Configures the predicate that decides whether an active state postpones processing an event.</summary>
    /// <typeparam name="TEvent">Deferred event payload type.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.2.-In-State-Event-Deferrals" />
    public interface IEventDeferralBuilder<TEvent> :
        IEventDeferralGuard<TEvent, IEventDeferralBuilder<TEvent>>;

    /// <summary>Extends the guard of an inherited event-deferral rule.</summary>
    /// <typeparam name="TEvent">Deferred event payload type.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.2.-In-State-Event-Deferrals" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IOverridenEventDeferralBuilder<TEvent> :
        IEventDeferralGuard<TEvent, IOverridenEventDeferralBuilder<TEvent>>;
}
