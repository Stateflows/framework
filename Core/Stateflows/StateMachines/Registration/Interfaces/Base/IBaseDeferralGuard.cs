using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using ActionDelegateAsync = Stateflows.Actions.Registration.ActionDelegateAsync;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IBaseDeferralGuard<TEvent, out TReturn>
    {
        /// <summary>
        /// Adds a function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>async c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guardsAsync">The asynchronous guard functions.</param>
        TReturn AddGuard(params Func<IDeferralContext<TEvent>, Task<bool>>[] guardsAsync);

        /// <summary>
        /// Registers activity behavior as guard
        /// </summary>
        /// <param name="buildAction">Build action</param>
        /// <typeparam name="TActivity">Activity behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddGuardActivity<TActivity>()
            where TActivity : class, IActivity
        {
            var edge = ((IEdgeBuilder)this).Edge;
            var vertex = edge.Source;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.{edge.Trigger}";
            if (edge.Target != null)
            {
                activityName += $".{edge.Target}";
            }
            activityName += $".guard.{edge.Guards.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName));
            return AddGuard(c => StateMachineActivityExtensions.RunDeferralGuardActivityAsync(edge.Guards.Actions.Count, c, activityName));
        }

        /// <summary>
        /// Registers Activity behavior as guard
        /// </summary>
        /// <param name="activityBuildAction">Activity build action</param>
        public TReturn AddGuardActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            var edge = ((IEdgeBuilder)this).Edge;
            var vertex = edge.Source;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.{edge.Trigger}";
            if (edge.Target != null)
            {
                activityName += $".{edge.Target}";
            }
            activityName += $".guard.{edge.Guards.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction));
            return AddGuard(c => StateMachineActivityExtensions.RunDeferralGuardActivityAsync(edge.Guards.Actions.Count, c, activityName));
        }

        /// <summary>
        /// Registers action behavior as guard
        /// </summary>
        /// <typeparam name="TAction">Action behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddGuardAction<TAction>()
            where TAction : class, IAction
        {
            var edge = ((IEdgeBuilder)this).Edge;
            var vertex = edge.Source;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.{edge.Trigger}";
            if (edge.Target != null)
            {
                actionName += $".{edge.Target}";
            }
            actionName += $".guard.{edge.Guards.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName));
            return AddGuard(c => StateMachineActionExtensions.RunDeferralGuardActionAsync(edge.Guards.Actions.Count, c, actionName));
        }

        /// <summary>
        /// Registers Action behavior as guard
        /// </summary>
        /// <param name="actionDelegate">Action delegate</param>
        /// <param name="reentrant">Flag that determines if action delegate can be executed in parallel</param>
        public TReturn AddGuardAction(ActionDelegateAsync actionDelegate, bool reentrant = true)
        {
            var edge = ((IEdgeBuilder)this).Edge;
            var vertex = edge.Source;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.{edge.Trigger}";
            if (edge.Target != null)
            {
                actionName += $".{edge.Target}";
            }
            actionName += $".guard.{edge.Guards.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegate, reentrant));
            return AddGuard(c => StateMachineActionExtensions.RunDeferralGuardActionAsync(edge.Guards.Actions.Count, c, actionName));
        }

        /// <summary>
        /// Adds a function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guards">The guard functions.</param>
        [DebuggerHidden]
        public TReturn AddGuard(params Func<IDeferralContext<TEvent>, bool>[] guards)
            => AddGuard(guards.Select(guard => guard.ToAsync()).ToArray());

        /// <summary>
        /// Adds a typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddGuard<TGuard>()
            where TGuard : class, IDeferralGuard<TEvent>
            => AddGuard(async c => await (await ((BaseContext)c).Context.Executor.GetDeferralGuardAsync<TGuard, TEvent>(c)).GuardAsync(c.Event));

        /// <summary>
        /// Adds a negated function-based guard to the current transition.
        /// </summary>
        /// <param name="guardsAsync">The asynchronous guard functions.</param>
        [DebuggerHidden]
        TReturn AddNegatedGuard(params Func<IDeferralContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuard(guardsAsync.Select<Func<IDeferralContext<TEvent>, Task<bool>>, Func<IDeferralContext<TEvent>, Task<bool>>>(guardAsync => async c => !await guardAsync.Invoke(c)).ToArray());

        /// <summary>
        /// Adds a negated function-based guard to the current transition.
        /// </summary>
        /// <param name="guards">The guard functions.</param>
        [DebuggerHidden]
        public TReturn AddNegatedGuard(params Func<IDeferralContext<TEvent>, bool>[] guards)
            => AddNegatedGuard(guards.Select(guard => guard.ToAsync()).ToArray());
        
        /// <summary>
        /// Adds a negated typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddNegatedGuard<TGuard>()
            where TGuard : class, IDeferralGuard<TEvent>
            => AddGuard(async c => !await (await ((BaseContext)c).Context.Executor.GetDeferralGuardAsync<TGuard, TEvent>(c)).GuardAsync(c.Event)!);
    }
}
