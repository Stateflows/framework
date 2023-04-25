using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateEventsBuilderBase<TReturn>
    {
        #region Events
        TReturn AddOnEntry(Func<IStateActionContext, Task> actionAsync);

        TReturn AddOnExit(Func<IStateActionContext, Task> actionAsync);
        #endregion
    }
}
