using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IBaseGuard<out TEvent, out TReturn>
    {
        TReturn AddGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync);
        
        [DebuggerHidden]
        public TReturn AddGuard(Func<ITransitionContext<TEvent>, bool> guard)
            => AddGuard(guard
                .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                .ToAsync()
            );
        
        [DebuggerHidden]
        TReturn AddGuard<TGuard>()
            where TGuard : class, ITransitionGuard<TEvent>
            => AddGuard(c => ((BaseContext)c).Context.Executor.GetTransitionGuard<TGuard, TEvent>(c)?.GuardAsync(c.Event));

        [DebuggerHidden]
        TReturn AddNegatedGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync)
            => AddGuard(async c => !await guardAsync.Invoke(c));
        
        [DebuggerHidden]
        public TReturn AddNegatedGuard(Func<ITransitionContext<TEvent>, bool> guard)
            => AddNegatedGuard(guard
                .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                .ToAsync()
            );
        
        [DebuggerHidden]
        TReturn AddNegatedGuard<TGuard>()
            where TGuard : class, ITransitionGuard<TEvent>
            => AddGuard(async c => !await ((BaseContext)c).Context.Executor.GetTransitionGuard<TGuard, TEvent>(c)?.GuardAsync(c.Event)!);
    }
}
