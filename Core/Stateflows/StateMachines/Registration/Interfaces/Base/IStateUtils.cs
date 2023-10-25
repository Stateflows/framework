using Stateflows.Common;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateUtils<out TReturn>
    {
        #region Utils
        TReturn AddDeferredEvent<TEvent>() where TEvent : Event, new();
        #endregion
    }
}
