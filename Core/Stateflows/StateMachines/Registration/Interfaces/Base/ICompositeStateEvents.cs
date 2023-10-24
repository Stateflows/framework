using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ICompositeStateEvents<TReturn>
    {
        TReturn AddOnInitialize(Func<IStateActionContext, Task> actionAsync);

        TReturn AddOnFinalize(Func<IStateActionContext, Task> actionAsync);
    }
}
