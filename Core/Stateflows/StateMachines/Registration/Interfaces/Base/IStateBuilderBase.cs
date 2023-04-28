using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateBuilderBase<TReturn>
    {
        #region Events
        TReturn AddOnEntry(Func<IStateActionContext, Task> actionAsync);

        TReturn AddOnExit(Func<IStateActionContext, Task> actionAsync);
        #endregion

        #region Utils
        TReturn AddDeferredEvent<TEvent>() where TEvent : Event, new();
        #endregion
    }
}
