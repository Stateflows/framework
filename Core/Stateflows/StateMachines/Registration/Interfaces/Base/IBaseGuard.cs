using System;
using System.Diagnostics;
using System.Linq;
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
        /// <param name="guardsAsync">The asynchronous guard functions.</param>
        TReturn AddGuard(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync);

        /// <summary>
        /// Adds a function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guards">The guard functions.</param>
        [DebuggerHidden]
        public TReturn AddGuard(params Func<ITransitionContext<TEvent>, bool>[] guards)
            => AddGuard(
                guards.Select(guard => guard
                    .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                    .ToAsync()
                ).ToArray()

            );

        /// <summary>
        /// Adds a typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddGuard<TGuard>()
            where TGuard : class, ITransitionGuard<TEvent>
            => AddGuard(async c => await (await ((BaseContext)c).Context.Executor.GetTransitionGuardAsync<TGuard, TEvent>(c)).GuardAsync(c.Event));

        /// <summary>
        /// Adds a negated function-based guard to the current transition.
        /// </summary>
        /// <param name="guardsAsync">The asynchronous guard functions.</param>
        [DebuggerHidden]
        TReturn AddNegatedGuard(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuard(guardsAsync.Select<Func<ITransitionContext<TEvent>, Task<bool>>, Func<ITransitionContext<TEvent>, Task<bool>>>(guardAsync => async c => !await guardAsync.Invoke(c)).ToArray());

        /// <summary>
        /// Adds a negated function-based guard to the current transition.
        /// </summary>
        /// <param name="guards">The guard functions.</param>
        [DebuggerHidden]
        public TReturn AddNegatedGuard(params Func<ITransitionContext<TEvent>, bool>[] guards)
            => AddNegatedGuard(
                guards.Select(guard => guard
                    .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                    .ToAsync()
                ).ToArray()
            );

        /// <summary>
        /// Adds a negated typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddNegatedGuard<TGuard>()
            where TGuard : class, ITransitionGuard<TEvent>
            => AddGuard(async c => !await (await ((BaseContext)c).Context.Executor.GetTransitionGuardAsync<TGuard, TEvent>(c)).GuardAsync(c.Event)!);
    }
}
