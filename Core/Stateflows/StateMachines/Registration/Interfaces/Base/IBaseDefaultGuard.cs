using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IBaseDefaultGuard<out TReturn>
    {
        /// <summary>
        /// Adds a function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>async c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guardAsync">The asynchronous guard function.</param>
        TReturn AddGuard(Func<ITransitionContext<Completion>, Task<bool>> guardAsync);

        /// <summary>
        /// Adds a function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guard">The guard function.</param>
        [DebuggerHidden]
        public TReturn AddGuard(Func<ITransitionContext<Completion>, bool> guard)
            => AddGuard(guard
                .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                .ToAsync()
            );

        /// <summary>
        /// Adds a negated function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>async c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guardAsync">The asynchronous guard function.</param>
        TReturn AddNegatedGuard(Func<ITransitionContext<Completion>, Task<bool>> guardAsync)
            => AddGuard(async c => !await guardAsync.Invoke(c));

        /// <summary>
        /// Adds a negated function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guard">The guard function.</param>
        [DebuggerHidden]
        public TReturn AddNegatedGuard(Func<ITransitionContext<Completion>, bool> guard)
            => AddNegatedGuard(guard
                .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                .ToAsync()
            );

        /// <summary>
        /// Adds a typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        TReturn AddGuard<TGuard>()
            where TGuard : class, IDefaultTransitionGuard
            => AddGuard(c => ((BaseContext)c).Context.Executor.GetDefaultTransitionGuard<TGuard>(c).GuardAsync());

        /// <summary>
        /// Adds a negated typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        TReturn AddNegatedGuard<TGuard>()
            where TGuard : class, IDefaultTransitionGuard
            => AddGuard(async c => !await ((BaseContext)c).Context.Executor.GetDefaultTransitionGuard<TGuard>(c).GuardAsync());
    }
}
