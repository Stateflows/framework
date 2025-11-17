using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IDeferralBuilder<TEvent> :
        IDeferralGuard<TEvent, IDeferralBuilder<TEvent>>
    { }

    public interface IOverridenDeferralBuilder<TEvent> :
        IDeferralGuard<TEvent, IOverridenDeferralBuilder<TEvent>>
    { }
}
