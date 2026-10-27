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
    /// <summary>Registers transition predicates, including injected delegates and embedded behavior predicates.
    /// </summary>
    /// <typeparam name="TEvent">Trigger event payload type available to the guard or effect through its execution
    /// context.</typeparam>
    /// <typeparam name="TReturn">The fluent builder returned after configuring the guard.</typeparam>
    /// <remarks>Each added guard must accept the transition for it to be eligible. See
    /// <see href="https://github.com/Stateflows/framework/wiki/state-machine-transition">transitions</see>.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
    public interface IBaseGuard<out TEvent, out TReturn>
    {
        /// <summary>Registers predicates for the current transition.</summary>
        /// <param name="guardAsync">Asynchronous predicate receiving the transition context. Must return true to permit the
        /// transition.</param>
        /// <returns>The current fluent builder with the predicate registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync)
            => AddGuards(guardAsync);
        
        /// <summary>Registers predicates for the current transition.</summary>
        /// <param name="guardsAsync">Asynchronous predicates receiving the transition context. Each must return true to
        /// permit the transition.</param>
        /// <returns>The current fluent builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddGuards(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync);

        /// <summary>Registers predicates for the current transition.</summary>
        /// <param name="guardDelegate">Predicate returning <c>bool</c> or <c>Task&lt;bool&gt;</c>; parameters are resolved
        /// from execution services and supported contexts.</param>
        /// <returns>The current fluent builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
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

        /// <summary>Runs a new embedded Activity whenever this guard is evaluated.</summary>
        /// <param name="buildAction">Optional callback configuring the typed embedded Activity's utilities and middleware
        /// without redefining its graph.</param>
        /// <typeparam name="TActivity">Embedded behavior implementation of <see cref="IActivity" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <returns>The current fluent builder with the activity guard registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
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

        /// <summary>Runs a new embedded Activity whenever this guard is evaluated.</summary>
        /// <param name="activityBuildAction">Callback configuring the embedded Activity's nodes, flows, and lifecycle.
        /// </param>
        /// <returns>The current fluent builder with the activity guard registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
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

        /// <summary>Runs a new embedded Action whenever this guard is evaluated.</summary>
        /// <typeparam name="TAction">Embedded behavior implementation of <see cref="IAction" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The current fluent builder with the action guard registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
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

        /// <summary>Runs a new embedded Action whenever this guard is evaluated.</summary>
        /// <param name="actionDelegate">Asynchronous implementation receiving the embedded Action's execution context.
        /// </param>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The current fluent builder with the action guard registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
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

        /// <summary>Registers predicates for the current transition.</summary>
        /// <param name="guard">Synchronous predicate receiving the transition context. Must return true to permit the
        /// transition.</param>
        /// <returns>The current fluent builder with the predicate registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        [DebuggerHidden]
        public TReturn AddGuard(Func<ITransitionContext<TEvent>, bool> guard)
            => AddGuards(guard);
        
        /// <summary>Registers predicates for the current transition.</summary>
        /// <param name="guards">Synchronous predicates receiving the transition context. Each must return true to permit
        /// the transition.</param>
        /// <returns>The current fluent builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        public TReturn AddGuards(params Func<ITransitionContext<TEvent>, bool>[] guards)
            => AddGuards(guards.Select(guard => guard.ToAsync()).ToArray());

        /// <summary>Registers typed predicates for the current transition.</summary>
        /// <typeparam name="TGuard">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result must
        /// be true for this predicate to accept.</typeparam>
        /// <param name="buildAction">Optional callback configuring the activated typed handler before its handler method is
        /// invoked.</param>
        /// <returns>The current fluent builder with the typed guard registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        [DebuggerHidden]
        TReturn AddGuard<TGuard>(ElementBuildAction<TGuard> buildAction = null)
            where TGuard : class, ITransitionGuard<TEvent>
            => AddGuard(async c => 
            {
                var state = await ((BaseContext)c).Context.Executor.GetTransitionGuardAsync<TGuard, TEvent>(c);
                buildAction?.Invoke(new ElementBuilder<TGuard>(state));
                return await state.GuardAsync(c.Event);
            });

        /// <summary>Registers negated predicates for the current transition.</summary>
        /// <param name="guardAsync">Asynchronous predicate receiving the transition context. Must return false for the
        /// negated guard to permit the transition.</param>
        /// <returns>The current fluent builder with the inverted predicate registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        [DebuggerHidden]
        TReturn AddNegatedGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync)
            => AddNegatedGuards(guardAsync);
        
        /// <summary>Registers negated predicates for the current transition.</summary>
        /// <param name="guardsAsync">Asynchronous predicates receiving the transition context. Each must return false for
        /// the negated guard to permit the transition.</param>
        /// <returns>The current fluent transition builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddNegatedGuards(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuards(guardsAsync.Select<Func<ITransitionContext<TEvent>, Task<bool>>, Func<ITransitionContext<TEvent>, Task<bool>>>(guardAsync => async c => !await guardAsync.Invoke(c)).ToArray());

        /// <summary>Registers negated predicates for the current transition.</summary>
        /// <param name="guard">Synchronous predicate receiving the transition context. Must return false for the negated
        /// guard to permit the transition.</param>
        /// <returns>The current fluent builder with the inverted predicate registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        [DebuggerHidden]
        public TReturn AddNegatedGuard(Func<ITransitionContext<TEvent>, bool> guard)
            => AddNegatedGuards(guard);
        
        /// <summary>Registers negated predicates for the current transition.</summary>
        /// <param name="guards">Synchronous predicates receiving the transition context. Each must return false for the
        /// negated guard to permit the transition.</param>
        /// <returns>The current fluent transition builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        public TReturn AddNegatedGuards(params Func<ITransitionContext<TEvent>, bool>[] guards)
            => AddNegatedGuards(guards.Select(guard => guard.ToAsync()).ToArray());
        
        /// <summary>Registers negated typed predicates for the current transition.</summary>
        /// <typeparam name="TGuard">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result must
        /// be false for this predicate to accept.</typeparam>
        /// <param name="buildAction">Optional callback configuring the activated typed handler before its handler method is
        /// invoked.</param>
        /// <returns>The current fluent builder with the inverted typed guard registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
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
