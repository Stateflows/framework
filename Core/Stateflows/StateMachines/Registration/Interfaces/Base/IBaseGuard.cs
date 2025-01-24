using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IBaseGuard<out TEvent, out TReturn>
    {
        /// <summary>
        /// Adds a function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>async c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guardAsync">The asynchronous guard function.</param>
        TReturn AddGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync);

        /// <summary>
        /// Adds a function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guard">The guard function.</param>
        [DebuggerHidden]
        public TReturn AddGuard(Func<ITransitionContext<TEvent>, bool> guard)
            => AddGuard(guard
                .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                .ToAsync()
            );

        /// <summary>
        /// Adds a typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddGuard<TGuard>()
            where TGuard : class, ITransitionGuard<TEvent>
            => AddGuard(c => ((BaseContext)c).Context.Executor.GetTransitionGuard<TGuard, TEvent>(c)?.GuardAsync(c.Event));

        /// <summary>
        /// Adds a negated function-based guard to the current transition.
        /// </summary>
        /// <param name="guardAsync">The asynchronous guard function.</param>
        [DebuggerHidden]
        TReturn AddNegatedGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync)
            => AddGuard(async c => !await guardAsync.Invoke(c));

        /// <summary>
        /// Adds a negated function-based guard to the current transition.
        /// </summary>
        /// <param name="guard">The guard function.</param>
        [DebuggerHidden]
        public TReturn AddNegatedGuard(Func<ITransitionContext<TEvent>, bool> guard)
            => AddNegatedGuard(guard
                .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                .ToAsync()
            );

        /// <summary>
        /// Adds a negated typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddNegatedGuard<TGuard>()
            where TGuard : class, ITransitionGuard<TEvent>
            => AddGuard(async c => !await ((BaseContext)c).Context.Executor.GetTransitionGuard<TGuard, TEvent>(c)?.GuardAsync(c.Event)!);
    }
}
