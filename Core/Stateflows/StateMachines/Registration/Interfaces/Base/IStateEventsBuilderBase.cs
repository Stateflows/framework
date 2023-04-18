using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateEventsBuilderBase<TReturn>
    {
        #region Events
        TReturn AddOnEntry(StateActionDelegateAsync actionAsync);

        TReturn AddOnExit(StateActionDelegateAsync actionAsync);
        #endregion
    }
}
