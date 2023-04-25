using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ICompositeStateEventsBuilderBase<TReturn>
    {
        #region Events
        TReturn AddOnInitialize(Func<IStateActionContext, Task> actionAsync);
        #endregion
    }
}
