﻿using Stateflows.Common.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineContext : IBehaviorContext
    {
        new StateMachineId Id { get; }
    }
}