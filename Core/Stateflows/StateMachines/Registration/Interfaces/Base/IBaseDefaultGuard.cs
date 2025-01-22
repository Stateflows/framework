using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IBaseDefaultGuard<out TReturn>
    {
        TReturn AddGuard(Func<ITransitionContext<Completion>, Task<bool>> guardAsync);
        
        [DebuggerHidden]
        public TReturn AddGuard(Func<ITransitionContext<Completion>, bool> guard)
            => AddGuard(guard
                .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                .ToAsync()
            );

        TReturn AddNegatedGuard(Func<ITransitionContext<Completion>, Task<bool>> guardAsync)
            => AddGuard(async c => !await guardAsync.Invoke(c));
        
        [DebuggerHidden]
        public TReturn AddNegatedGuard(Func<ITransitionContext<Completion>, bool> guard)
            => AddNegatedGuard(guard
                .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                .ToAsync()
            );
        
        TReturn AddGuard<TGuard>()
            where TGuard : class, IDefaultTransitionGuard
            => AddGuard(c => ((BaseContext)c).Context.Executor.GetDefaultTransitionGuard<TGuard>(c).GuardAsync());
        
        TReturn AddNegatedGuard<TGuard>()
            where TGuard : class, IDefaultTransitionGuard
            => AddGuard(async c => !await ((BaseContext)c).Context.Executor.GetDefaultTransitionGuard<TGuard>(c).GuardAsync());
    }
}
