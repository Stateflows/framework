namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateUtils<out TReturn>
    {
        TReturn AddDeferredEvent<TEvent>();
    }
}
