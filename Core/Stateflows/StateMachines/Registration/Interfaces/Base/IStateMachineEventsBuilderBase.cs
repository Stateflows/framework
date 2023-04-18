using Stateflows.StateMachines.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineEventsBuilderBase<TReturn>
    {
        #region Events
        TReturn AddOnInitialize(StateMachineActionDelegateAsync actionAsync);
        #endregion
    }
}
