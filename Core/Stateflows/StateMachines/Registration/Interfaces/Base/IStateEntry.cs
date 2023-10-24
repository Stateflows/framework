using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateEntry<TReturn>
    {
        TReturn AddOnEntry(Func<IStateActionContext, Task> actionAsync);
    }
}
