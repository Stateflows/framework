﻿using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateMachineAction
    {
        public static Func<IStateMachineInitializationContext, Task<bool>> ToAsync(this Func<IStateMachineInitializationContext, bool> stateMachineAction)
            => c => Task.Run(() => stateMachineAction(c));

        public static Func<IStateMachineInitializationContext<TInitializationEvent>, Task<bool>> ToAsync<TInitializationEvent>(this Func<IStateMachineInitializationContext<TInitializationEvent>, bool> stateMachineAction)
            where TInitializationEvent : Event, new()
            => c => Task.Run(() => stateMachineAction(c));

        public static Func<IStateMachineActionContext, Task> ToAsync(this Action<IStateMachineActionContext> stateMachineAction)
            => c => Task.Run(() => stateMachineAction(c));
    }
}
