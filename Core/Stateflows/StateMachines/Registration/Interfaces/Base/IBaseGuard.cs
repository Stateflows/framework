using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Builders;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using ActionBuildAction = Stateflows.Actions.Registration.Interfaces.ActionBuildAction;
using ActionDelegateAsync = Stateflows.Actions.Registration.ActionDelegateAsync;

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
        TReturn AddGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync)
            => AddGuards(guardAsync);
        
        TReturn AddGuards(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync);

        TReturn AddGuard(Delegate guardDelegate)
            => AddGuard(c => guardDelegate.InvokeDelegatePredicateAsync(
                StateflowsActivator.ResolveParameterValueFactories(
                    c.Behavior.ServiceProvider,
                    null,
                    "transition guard",
                    guardDelegate.Method.GetParameters()
                )
            ));

        // TReturn AddGuard(Func<TEvent, bool> guard)
        //     => AddGuard(c => guard.Invoke(c.Event));
        //
        // TReturn AddGuard(Func<TEvent, Task<bool>> guardAsync)
        //     => AddGuard(c => guardAsync.Invoke(c.Event));

        /// <summary>
        /// Registers activity behavior as guard
        /// </summary>
        /// <param name="buildAction">Build action</param>
        /// <typeparam name="TActivity">Activity behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddGuardActivity<TActivity>(ActivityUtilsBuildAction buildAction = null)
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
            
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddGuard(c => StateMachineActivityExtensions.RunTransitionGuardActivityAsync(edge.Guards.Actions.Count, c, activityName));
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
            
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddGuard(c => StateMachineActivityExtensions.RunTransitionGuardActivityAsync(edge.Guards.Actions.Count, c, activityName));
        }

        /// <summary>
        /// Registers action behavior as guard
        /// </summary>
        /// <typeparam name="TAction">Action behavior type</typeparam>
        /// <param name="buildAction">Build action</param>
        [DebuggerHidden]
        public TReturn AddGuardAction<TAction>(ActionBuildAction<TAction> buildAction = null)
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
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddGuard(c => StateMachineActionExtensions.RunTransitionGuardActionAsync(edge.Guards.Actions.Count, c, actionName));
        }

        /// <summary>
        /// Registers Action behavior as guard
        /// </summary>
        /// <param name="actionDelegate">Action delegate</param>
        /// <param name="buildAction">Build action</param>
        public TReturn AddGuardAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null)
        {
            var edge = ((IEdgeBuilder)this).Edge;
            var vertex = edge.Source;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.{edge.Trigger}";
            if (edge.Target != null)
            {
                actionName += $".{edge.Target}";
            }
            actionName += $".guard.{edge.Guards.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegate, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddGuard(c => StateMachineActionExtensions.RunTransitionGuardActionAsync(edge.Guards.Actions.Count, c, actionName));
        }

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
            => AddGuards(guard);
        
        public TReturn AddGuards(params Func<ITransitionContext<TEvent>, bool>[] guards)
            => AddGuards(guards.Select(guard => guard.ToAsync()).ToArray());

        /// <summary>
        /// Adds a typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddGuard<TGuard>(ElementBuildAction<TGuard> buildAction = null)
            where TGuard : class, ITransitionGuard<TEvent>
            => AddGuard(async c => 
            {
                var state = await ((BaseContext)c).Context.Executor.GetTransitionGuardAsync<TGuard, TEvent>(c);
                buildAction?.Invoke(new ElementBuilder<TGuard>(state));
                return await state.GuardAsync(c.Event);
            });

        /// <summary>
        /// Adds a negated function-based guard to the current transition.
        /// </summary>
        /// <param name="guardAsync">The asynchronous guard function.</param>
        [DebuggerHidden]
        TReturn AddNegatedGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync)
            => AddNegatedGuards(guardAsync);
        
        TReturn AddNegatedGuards(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuards(guardsAsync.Select<Func<ITransitionContext<TEvent>, Task<bool>>, Func<ITransitionContext<TEvent>, Task<bool>>>(guardAsync => async c => !await guardAsync.Invoke(c)).ToArray());

        /// <summary>
        /// Adds a negated function-based guard to the current transition.
        /// </summary>
        /// <param name="guard">The guard function.</param>
        [DebuggerHidden]
        public TReturn AddNegatedGuard(Func<ITransitionContext<TEvent>, bool> guard)
            => AddNegatedGuards(guard);
        
        public TReturn AddNegatedGuards(params Func<ITransitionContext<TEvent>, bool>[] guards)
            => AddNegatedGuards(guards.Select(guard => guard.ToAsync()).ToArray());
        
        /// <summary>
        /// Adds a negated typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddNegatedGuard<TGuard>(ElementBuildAction<TGuard> buildAction = null)
            where TGuard : class, ITransitionGuard<TEvent>
            => AddGuard(async c => 
            {
                var state = await ((BaseContext)c).Context.Executor.GetTransitionGuardAsync<TGuard, TEvent>(c);
                buildAction?.Invoke(new ElementBuilder<TGuard>(state));
                return !await state.GuardAsync(c.Event);
            });
    }
}
