namespace Stateflows.Actions.Registration.Interfaces.Base;

public interface IActionUtils<out TReturn>
{
    TReturn SetResourceName(string resourceName);
    TReturn SetCustomBehaviorClassType(string behaviorClassType);
    TReturn SetIsStateless(bool isStateless);
    TReturn AddConsumedEvent<TEvent>();
    TReturn AddProducedEvent<TEvent>();
    TReturn AddConsumedToken<TToken>();
    TReturn AddProducedToken<TToken>();
}
