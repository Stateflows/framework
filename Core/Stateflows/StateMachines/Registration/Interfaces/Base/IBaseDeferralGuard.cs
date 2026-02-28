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
    public interface IBaseDeferralGuard<TEvent, out TReturn>
    {
        /// <summary>
        /// Adds a function-based guard to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>async c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="guardAsync">The asynchronous guard function.</param>
        TReturn AddGuard(Func<IDeferralContext<TEvent>, Task<bool>> guardAsync)
            => AddGuards(guardAsync);
        
        TReturn AddGuards(params Func<IDeferralContext<TEvent>, Task<bool>>[] guardsAsync);

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

        /// <summary>
        /// Registers activity behavior as guard
        /// </summary>
        /// <param name="buildAction">Build action</param>
        /// <typeparam name="TActivity">Activity behavior type</typeparam>
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

        /// <summary>
        /// Registers Activity behavior as guard
        /// </summary>
        /// <param name="activityBuildAction">Activity build action</param>
        public TReturn AddGuardActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            var deferral = (IDeferralBuilder)this;
            var vertex = deferral.Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.{Event.GetName(deferral.EventType)}.deferral.{deferral.Guards.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddGuard(c => StateMachineActivityExtensions.RunDeferralGuardActivityAsync(deferral.Guards.Actions.Count, c, activityName));
        }

        /// <summary>
        /// Registers action behavior as guard
        /// </summary>
        /// <typeparam name="TAction">Action behavior type</typeparam>
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

        /// <summary>
        /// Registers Action behavior as guard
        /// </summary>
        /// <param name="actionDelegate">Action delegate</param>
        /// <param name="buildAction">Build action</param>
        public TReturn AddGuardAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null)
        {
            var deferral = (IDeferralBuilder)this;
            var vertex = deferral.Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.{Event.GetName(deferral.EventType)}.deferral.{deferral.Guards.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegate, buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddGuard(c => StateMachineActionExtensions.RunDeferralGuardActionAsync(deferral.Guards.Actions.Count, c, actionName));
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
        public TReturn AddGuard(Func<IDeferralContext<TEvent>, bool> guard)
            => AddGuards(guard);
        
        public TReturn AddGuards(params Func<IDeferralContext<TEvent>, bool>[] guards)
            => AddGuards(guards.Select(guard => guard.ToAsync()).ToArray());

        /// <summary>
        /// Adds a typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
        [DebuggerHidden]
        TReturn AddGuard<TGuard>(ElementBuildAction<TGuard> buildAction = null)
            where TGuard : class, IDeferralGuard<TEvent>
            => AddGuard(async c => 
            {
                var transition = await ((BaseContext)c).Context.Executor.GetDeferralGuardAsync<TGuard, TEvent>(c);
                buildAction?.Invoke(new ElementBuilder<TGuard>(transition));
                return await transition.GuardAsync(c.Event);
            });

        /// <summary>
        /// Adds a negated function-based guard to the current transition.
        /// </summary>
        /// <param name="guardAsync">The asynchronous guard function.</param>
        [DebuggerHidden]
        TReturn AddNegatedGuard(Func<IDeferralContext<TEvent>, Task<bool>> guardAsync)
            => AddNegatedGuards(guardAsync);
        
        TReturn AddNegatedGuards(params Func<IDeferralContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuards(guardsAsync.Select<Func<IDeferralContext<TEvent>, Task<bool>>, Func<IDeferralContext<TEvent>, Task<bool>>>(guardAsync => async c => !await guardAsync.Invoke(c)).ToArray());

        /// <summary>
        /// Adds a negated function-based guard to the current transition.
        /// </summary>
        /// <param name="guard">The guard function.</param>
        [DebuggerHidden]
        public TReturn AddNegatedGuard(Func<IDeferralContext<TEvent>, bool> guard)
            => AddNegatedGuards(guard);
        
        public TReturn AddNegatedGuards(params Func<IDeferralContext<TEvent>, bool>[] guards)
            => AddNegatedGuards(guards.Select(guard => guard.ToAsync()).ToArray());
        
        /// <summary>
        /// Adds a negated typed guard handler to the current transition.
        /// </summary>
        /// <typeparam name="TGuard">The type of the guard handler.</typeparam>
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
