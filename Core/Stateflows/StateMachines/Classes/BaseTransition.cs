﻿using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class BaseTransition<TEvent>
        where TEvent : Event, new()
    {
        public ITransitionContext<TEvent> Context { get; internal set; }

        public virtual Task<bool> GuardAsync()
            => Task.FromResult(true);

        public virtual Task EffectAsync()
            => Task.CompletedTask;
    }
}
