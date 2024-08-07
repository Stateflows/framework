﻿using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IExecutionContext
    {
        Event ExecutionTrigger { get; }
    }

    public interface IInitializationContext
    {
        Event InitializationEvent { get; }
    }

    public interface IStateMachineActionContext : IBehaviorLocator, IExecutionContext
    {
        IStateMachineContext StateMachine { get; }
    }
}
