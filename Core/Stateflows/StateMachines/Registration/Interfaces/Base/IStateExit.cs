using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateExit<TReturn>
    {
        TReturn AddOnExit(Func<IStateActionContext, Task> actionAsync);
    }
}
