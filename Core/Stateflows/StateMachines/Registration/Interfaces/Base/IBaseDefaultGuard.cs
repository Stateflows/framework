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
        TReturn AddGuard(Func<ITransitionContext<Completion>, Task<bool>> guardAsync)
            => AddGuards(guardAsync);
        
        TReturn AddGuards(params Func<ITransitionContext<Completion>, Task<bool>>[] guardsAsync);
        
        [DebuggerHidden]
        public TReturn AddGuards(params Func<ITransitionContext<Completion>, bool>[] guards)
            => AddGuards(guards.Select(guard => guard.ToAsync()).ToArray());

        TReturn AddGuard(Delegate guardDelegate)
            => AddGuard(c => guardDelegate.InvokeDelegatePredicateAsync(
                StateflowsActivator.ResolveParameterValueFactories(
                    c.Behavior.ServiceProvider,
                    null,
                    "transition guard",
                    guardDelegate.Method.GetParameters()
                )
            ));

        // TReturn AddGuard(Func<bool> guard)
        //     => AddGuard(c => guard.Invoke());
        //
        // TReturn AddGuard(Func<Task<bool>> guardAsync)
        //     => AddGuard(c => guardAsync.Invoke());
        
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
            => AddGuards(guard.ToAsync());

        /// <summary>
        /// Adds activity behavior as guard
        /// </summary>
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
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName, buildAction: buildAction, version: 1), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
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
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegate, buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddGuard(c => StateMachineActionExtensions.RunTransitionGuardActionAsync(edge.Guards.Actions.Count, c, actionName));
        }

        /// <summary>
        /// Adds a negated function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>async c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guardAsync">The asynchronous guard function.</param>
        [DebuggerHidden]
        TReturn AddNegatedGuard(Func<ITransitionContext<Completion>, Task<bool>> guardAsync)
            => AddNegatedGuards(guardAsync);
        
        TReturn AddNegatedGuards(params Func<ITransitionContext<Completion>, Task<bool>>[] guardsAsync)
            => AddGuards(guardsAsync.Select<Func<ITransitionContext<Completion>, Task<bool>>, Func<ITransitionContext<Completion>, Task<bool>>>(guardAsync => async c => !await guardAsync.Invoke(c)).ToArray());

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
            => AddNegatedGuards(guard);
        
        public TReturn AddNegatedGuards(params Func<ITransitionContext<Completion>, bool>[] guards)
            => AddNegatedGuards(guards.Select(guard => guard.ToAsync()).ToArray());

        /// <summary>
        /// Adds a typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddGuard<TGuard>(ElementBuildAction<TGuard> buildAction = null)
            where TGuard : class, IDefaultTransitionGuard
            => AddGuard(async c => 
            {
                var transition = await ((BaseContext)c).Context.Executor.GetDefaultTransitionGuardAsync<TGuard>(c);
                buildAction?.Invoke(new ElementBuilder<TGuard>(transition));
                return await transition.GuardAsync();
            });

        /// <summary>
        /// Adds a negated typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddNegatedGuard<TGuard>(ElementBuildAction<TGuard> buildAction = null)
            where TGuard : class, IDefaultTransitionGuard
            => AddGuard(async c =>  
            {
                var transition = await ((BaseContext)c).Context.Executor.GetDefaultTransitionGuardAsync<TGuard>(c);
                buildAction?.Invoke(new ElementBuilder<TGuard>(transition));
                return !await transition.GuardAsync();
            });
    }
}
