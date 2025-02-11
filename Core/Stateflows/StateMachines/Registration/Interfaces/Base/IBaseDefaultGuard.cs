using System;
using System.Diagnostics;
using System.Linq;
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
        /// <param name="guardsAsync">The asynchronous guard functions.</param>
        TReturn AddGuard(params Func<ITransitionContext<Completion>, Task<bool>>[] guardsAsync);

        /// <summary>
        /// Adds a function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guards">The guard functions.</param>
        [DebuggerHidden]
        public TReturn AddGuard(params Func<ITransitionContext<Completion>, bool>[] guards)
            => AddGuard(
                guards.Select(guard => guard
                
                    .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                    .ToAsync()
                ).ToArray()
            );


        /// <summary>
        /// Adds a negated function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>async c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guardsAsync">The asynchronous guard functions.</param>
        [DebuggerHidden]
        TReturn AddNegatedGuard(params Func<ITransitionContext<Completion>, Task<bool>>[] guardsAsync)
            => AddGuard(guardsAsync.Select<Func<ITransitionContext<Completion>, Task<bool>>, Func<ITransitionContext<Completion>, Task<bool>>>(guardAsync => async c => !await guardAsync.Invoke(c)).ToArray());

        /// <summary>
        /// Adds a negated function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guards">The guard functions.</param>
        [DebuggerHidden]
        public TReturn AddNegatedGuard(params Func<ITransitionContext<Completion>, bool>[] guards)
            => AddNegatedGuard(
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
            where TGuard : class, IDefaultTransitionGuard
            => AddGuard(async c => await (await ((BaseContext)c).Context.Executor.GetDefaultTransitionGuardAsync<TGuard>(c)).GuardAsync());

        /// <summary>
        /// Adds a negated typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddNegatedGuard<TGuard>()
            where TGuard : class, IDefaultTransitionGuard
            => AddGuard(async c => !await (await ((BaseContext)c).Context.Executor.GetDefaultTransitionGuardAsync<TGuard>(c)).GuardAsync());
    }
}
