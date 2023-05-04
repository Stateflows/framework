using Stateflows.Common;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateUtilsBuilderBase<TReturn>
    {
        #region Utils
        TReturn AddDeferredEvent<TEvent>() where TEvent : Event, new();
        #endregion
    }
}
