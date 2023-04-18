using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ICompositeStateEventsBuilderBase<TReturn>
    {
        #region Events
        TReturn AddOnInitialize(StateActionDelegateAsync actionAsync);
        #endregion
    }
}
