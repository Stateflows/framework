using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
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
    /// <summary>Registers predicates that decide whether an active state defers a candidate event.</summary>
    /// <typeparam name="TEvent">Deferred event payload type.</typeparam>
    /// <typeparam name="TReturn">The fluent deferral builder returned after configuration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
    public interface IBaseDeferralGuard<TEvent, out TReturn>
    {
        /// <summary>Registers predicates for the current deferral rule.</summary>
        /// <param name="guardAsync">Asynchronous predicate receiving the deferral context. Must return true to permit
        /// deferral.</param>
        /// <returns>The current deferral builder with the predicate registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        TReturn AddGuard(Func<IDeferralContext<TEvent>, Task<bool>> guardAsync)
            => AddGuards(guardAsync);
        
        /// <summary>Registers predicates for the current deferral rule.</summary>
        /// <param name="guardsAsync">Asynchronous predicates receiving the deferral context. Each must return true to
        /// permit deferral.</param>
        /// <returns>The current fluent deferral builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        TReturn AddGuards(params Func<IDeferralContext<TEvent>, Task<bool>>[] guardsAsync);

        /// <summary>Registers predicates for the current deferral rule.</summary>
        /// <param name="guardDelegate">Predicate returning <c>bool</c> or <c>Task&lt;bool&gt;</c>; parameters are resolved
        /// from execution services and supported contexts.</param>
        /// <returns>The current fluent deferral builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        TReturn AddGuard(Delegate guardDelegate)
            => AddGuard(c => guardDelegate.InvokeDelegatePredicateAsync(
                StateflowsActivator.ResolveParameterValueFactories(
                    c.Behavior.ServiceProvider,
                    null,
                    "deferral guard",
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
        /// <returns>The current deferral builder with the activity guard registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddGuardActivity<TActivity>(ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity
        {
            var deferral = (IDeferralBuilder)this;
            var vertex = deferral.Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.{Event.GetName(deferral.EventType)}.deferral.{deferral.Guards.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddGuard(c => StateMachineActivityExtensions.RunDeferralGuardActivityAsync(deferral.Guards.Actions.Count, c, activityName));
        }

        /// <summary>Runs a new embedded Activity whenever this guard is evaluated.</summary>
        /// <param name="activityBuildAction">Callback configuring the embedded Activity's nodes, flows, and lifecycle.
        /// </param>
        /// <returns>The current deferral builder with the activity guard registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddGuardActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            var deferral = (IDeferralBuilder)this;
            var vertex = deferral.Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.{Event.GetName(deferral.EventType)}.deferral.{deferral.Guards.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddGuard(c => StateMachineActivityExtensions.RunDeferralGuardActivityAsync(deferral.Guards.Actions.Count, c, activityName));
        }

        /// <summary>Runs a new embedded Action whenever this guard is evaluated.</summary>
        /// <typeparam name="TAction">Embedded behavior implementation of <see cref="IAction" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The current deferral builder with the action guard registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddGuardAction<TAction>(ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction
        {
            var deferral = (IDeferralBuilder)this;
            var vertex = deferral.Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.{Event.GetName(deferral.EventType)}.deferral.{deferral.Guards.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddGuard(c => StateMachineActionExtensions.RunDeferralGuardActionAsync(deferral.Guards.Actions.Count, c, actionName));
        }

        /// <summary>Runs a new embedded Action whenever this guard is evaluated.</summary>
        /// <param name="actionDelegate">Asynchronous implementation receiving the embedded Action's execution context.
        /// </param>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The current deferral builder with the action guard registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddGuardAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null)
        {
            var deferral = (IDeferralBuilder)this;
            var vertex = deferral.Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.{Event.GetName(deferral.EventType)}.deferral.{deferral.Guards.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegate, buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddGuard(c => StateMachineActionExtensions.RunDeferralGuardActionAsync(deferral.Guards.Actions.Count, c, actionName));
        }

        /// <summary>Registers predicates for the current deferral rule.</summary>
        /// <param name="guard">Synchronous predicate receiving the deferral context. Must return true to permit deferral.
        /// </param>
        /// <returns>The current deferral builder with the predicate registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        [DebuggerHidden]
        public TReturn AddGuard(Func<IDeferralContext<TEvent>, bool> guard)
            => AddGuards(guard);
        
        /// <summary>Registers predicates for the current deferral rule.</summary>
        /// <param name="guards">Synchronous predicates receiving the deferral context. Each must return true to permit
        /// deferral.</param>
        /// <returns>The current fluent deferral builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        public TReturn AddGuards(params Func<IDeferralContext<TEvent>, bool>[] guards)
            => AddGuards(guards.Select(guard => guard.ToAsync()).ToArray());

        /// <summary>Registers typed predicates for the current deferral rule.</summary>
        /// <typeparam name="TGuard">Typed guard implementing <see cref="IDeferralGuard&lt;TEvent&gt;" />. Its result must
        /// be true for this predicate to accept.</typeparam>
        /// <param name="buildAction">Optional callback configuring the activated typed handler before its handler method is
        /// invoked.</param>
        /// <returns>The current deferral builder with the typed predicate registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        [DebuggerHidden]
        TReturn AddGuard<TGuard>(ElementBuildAction<TGuard> buildAction = null)
            where TGuard : class, IDeferralGuard<TEvent>
            => AddGuard(async c => 
            {
                var transition = await ((BaseContext)c).Context.Executor.GetDeferralGuardAsync<TGuard, TEvent>(c);
                buildAction?.Invoke(new ElementBuilder<TGuard>(transition));
                return await transition.GuardAsync(c.Event);
            });

        /// <summary>Registers negated predicates for the current deferral rule.</summary>
        /// <param name="guardAsync">Asynchronous predicate receiving the deferral context. Must return false for the
        /// negated guard to permit deferral.</param>
        /// <returns>The current deferral builder with the inverted predicate registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        [DebuggerHidden]
        TReturn AddNegatedGuard(Func<IDeferralContext<TEvent>, Task<bool>> guardAsync)
            => AddNegatedGuards(guardAsync);
        
        /// <summary>Registers negated predicates for the current deferral rule.</summary>
        /// <param name="guardsAsync">Asynchronous predicates receiving the deferral context. Each must return false for the
        /// negated guard to permit deferral.</param>
        /// <returns>The current fluent deferral builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        TReturn AddNegatedGuards(params Func<IDeferralContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuards(guardsAsync.Select<Func<IDeferralContext<TEvent>, Task<bool>>, Func<IDeferralContext<TEvent>, Task<bool>>>(guardAsync => async c => !await guardAsync.Invoke(c)).ToArray());

        /// <summary>Registers negated predicates for the current deferral rule.</summary>
        /// <param name="guard">Synchronous predicate receiving the deferral context. Must return false for the negated
        /// guard to permit deferral.</param>
        /// <returns>The current deferral builder with the inverted predicate registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        [DebuggerHidden]
        public TReturn AddNegatedGuard(Func<IDeferralContext<TEvent>, bool> guard)
            => AddNegatedGuards(guard);
        
        /// <summary>Registers negated predicates for the current deferral rule.</summary>
        /// <param name="guards">Synchronous predicates receiving the deferral context. Each must return false for the
        /// negated guard to permit deferral.</param>
        /// <returns>The current fluent deferral builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        public TReturn AddNegatedGuards(params Func<IDeferralContext<TEvent>, bool>[] guards)
            => AddNegatedGuards(guards.Select(guard => guard.ToAsync()).ToArray());
        
        /// <summary>Registers negated typed predicates for the current deferral rule.</summary>
        /// <typeparam name="TGuard">Typed guard implementing <see cref="IDeferralGuard&lt;TEvent&gt;" />. Its result must
        /// be false for this predicate to accept.</typeparam>
        /// <param name="buildAction">Optional callback configuring the activated typed handler before its handler method is
        /// invoked.</param>
        /// <returns>The current deferral builder with the inverted typed predicate registered.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        [DebuggerHidden]
        TReturn AddNegatedGuard<TGuard>(ElementBuildAction<TGuard> buildAction = null)
            where TGuard : class, IDeferralGuard<TEvent>
            => AddGuard(async c => 
            {
                var transition = await ((BaseContext)c).Context.Executor.GetDeferralGuardAsync<TGuard, TEvent>(c);
                buildAction?.Invoke(new ElementBuilder<TGuard>(transition));
                return !await transition.GuardAsync(c.Event);
            });
    }
}
