using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IBaseGuard<TEvent, out TReturn>
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
        /// Adds activity behavior as guard
        /// </summary>
        /// <param name="activityName">Activity behavior name</param>
        /// <param name="buildAction">Build action</param>
        [DebuggerHidden]
        public TReturn AddGuardActivity(string activityName, TransitionActivityBuildAction<TEvent> buildAction = null)
        {
            var guardIndex = (this as IEdgeBuilder).Edge.Guards.Actions.Count;
            
            return AddGuard(c => StateMachineActivityExtensions.RunGuardActivityAsync(guardIndex, c, activityName, buildAction));
        }

        /// <summary>
        /// Adds activity behavior as guard
        /// </summary>
        /// <param name="buildAction">Build action</param>
        /// <typeparam name="TActivity">Activity behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddGuardActivity<TActivity>(TransitionActivityBuildAction<TEvent> buildAction = null)
            where TActivity : class, IActivity
            => AddGuardActivity(Activity<TActivity>.Name, buildAction);

        /// <summary>
        /// Adds action behavior as guard
        /// </summary>
        /// <param name="actionName">Action behavior name</param>
        /// <param name="buildAction">Build action</param>
        [DebuggerHidden]
        public TReturn AddGuardAction(string actionName, TransitionActionBuildAction<TEvent> buildAction = null)
        {
            var guardIndex = (this as IEdgeBuilder).Edge.Guards.Actions.Count;

            return AddGuard(c => StateMachineActionExtensions.RunGuardActionAsync(guardIndex, c, actionName, buildAction));
        }

        /// <summary>
        /// Adds action behavior as guard
        /// </summary>
        /// <param name="buildAction">Build action</param>
        /// <typeparam name="TAction">Action behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddGuardAction<TAction>(TransitionActionBuildAction<TEvent> buildAction = null)
            where TAction : class, IAction
            => AddGuardAction(Stateflows.Actions.Action<TAction>.Name, buildAction);

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
