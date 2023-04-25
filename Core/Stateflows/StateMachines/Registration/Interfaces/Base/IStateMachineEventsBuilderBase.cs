using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineEventsBuilderBase<TReturn>
    {
        #region Events
        TReturn AddOnInitialize(Func<IStateMachineActionContext, Task> actionAsync);
        #endregion
    }
}
