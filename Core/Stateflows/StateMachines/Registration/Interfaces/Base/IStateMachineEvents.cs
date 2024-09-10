using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineEvents<TReturn>
    {
        TReturn AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync);

        TReturn AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync);

        TReturn AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync);
    }
}
