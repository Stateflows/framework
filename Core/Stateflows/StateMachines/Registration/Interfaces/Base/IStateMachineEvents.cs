using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineEvents<TReturn>
    {
        TReturn AddDefaultInitializer(Func<IStateMachineInitializationContext, Task<bool>> actionAsync);

        TReturn AddInitializer<TInitializationEvent>(Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> actionAsync)
            where TInitializationEvent : Event, new();

        TReturn AddFinalizer(Func<IStateMachineActionContext, Task> actionAsync);
    }
}
