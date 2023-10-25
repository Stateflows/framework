using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineEvents<TReturn>
    {
        TReturn AddOnInitialize(Func<IStateMachineInitializationContext, Task> actionAsync);

        TReturn AddOnInitialize<TInitializationRequest>(Func<IStateMachineInitializationContext<TInitializationRequest>, Task> actionAsync)
            where TInitializationRequest : InitializationRequest, new();

        TReturn AddOnFinalize(Func<IStateMachineActionContext, Task> actionAsync);
    }
}
