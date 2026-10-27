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
    /// <summary>Registers effects awaited when a triggered transition is selected.</summary>
    /// <typeparam name="TEvent">Trigger event payload type available to the guard or effect through its execution
    /// context.</typeparam>
    /// <typeparam name="TReturn">The fluent transition builder returned after registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
    public interface IEffect<out TEvent, out TReturn>
    {
        /// <summary>Registers effects to execute when the current transition is selected.</summary>
        /// <param name="effectsAsync">Asynchronous effects receiving the transition context; each returned task is awaited.
        /// </param>
        /// <returns>The current transition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync);

        /// <summary>Registers effects to execute when the current transition is selected.</summary>
        /// <param name="effectDelegate">Synchronous or asynchronous effect whose parameters are resolved from execution
        /// services and supported contexts.</param>
        /// <returns>The current fluent transition builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddEffect(Delegate effectDelegate)
            => AddEffect(c => effectDelegate.InvokeDelegateActionAsync(
                StateflowsActivator.ResolveParameterValueFactories(
                    c.Behavior.ServiceProvider,
                    null,
                    "transition effect",
                    effectDelegate.Method.GetParameters()
                )
            ));

        /// <summary>Runs a new embedded Activity whenever this transition effect executes.</summary>
        /// <typeparam name="TActivity">Embedded behavior implementation of <see cref="IActivity" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <param name="buildAction">Optional callback configuring the typed embedded Activity's utilities and middleware
        /// without redefining its graph.</param>
        /// <returns>The current transition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddEffectActivity<TActivity>(ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity
        {
            var edge = ((IEdgeBuilder)this).Edge;
            var vertex = edge.Source;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.{edge.Trigger}";
            if (edge.Target != null)
            {
                activityName += $".{edge.Target}";
            }
            activityName += $".effect.{edge.Effects.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddEffect(c => StateMachineActivityExtensions.RunEffectActivity(c, activityName));
        }

        /// <summary>Runs a new embedded Activity whenever this transition effect executes.</summary>
        /// <param name="activityBuildAction">Callback configuring the embedded Activity's nodes, flows, and lifecycle.
        /// </param>
        /// <returns>The current transition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddEffectActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            var edge = ((IEdgeBuilder)this).Edge;
            var vertex = edge.Source;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.{edge.Trigger}";
            if (edge.Target != null)
            {
                activityName += $".{edge.Target}";
            }
            activityName += $".effect.{edge.Effects.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddEffect(c => StateMachineActivityExtensions.RunEffectActivity(c, activityName));
        }
        
        /// <summary>Runs a new embedded Action whenever this transition effect executes.</summary>
        /// <typeparam name="TAction">Embedded behavior implementation of <see cref="IAction" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The current transition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddEffectAction<TAction>(ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction
        {
            var edge = ((IEdgeBuilder)this).Edge;
            var vertex = edge.Source;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.{edge.Trigger}";
            if (edge.Target != null)
            {
                actionName += $".{edge.Target}";
            }
            actionName += $".effect.{edge.Effects.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddEffect(c => StateMachineActionExtensions.RunEffectActionAsync(c, actionName));
        }

        /// <summary>Runs a new embedded Action whenever this transition effect executes.</summary>
        /// <param name="actionDelegate">Asynchronous implementation receiving the embedded Action's execution context.
        /// </param>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The current transition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddEffectAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction = null)
        {
            var edge = ((IEdgeBuilder)this).Edge;
            var vertex = edge.Source;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.{edge.Trigger}";
            if (edge.Target != null)
            {
                actionName += $".{edge.Target}";
            }
            actionName += $".effect.{edge.Effects.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegate, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddEffect(c => StateMachineActionExtensions.RunEffectActionAsync(c, actionName));
        }
        
        /// <summary>Registers effects to execute when the current transition is selected.</summary>
        /// <param name="effects">Synchronous effects receiving the transition context.</param>
        /// <returns>The current transition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        [DebuggerHidden]
        public TReturn AddEffect(params System.Action<ITransitionContext<TEvent>>[] effects)
            => AddEffect(effects.Select(effect => effect.ToAsync()).ToArray());

        /// <summary>Registers typed effects for the current transition.</summary>
        /// <typeparam name="TEffect">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <param name="buildAction">Optional callback configuring the activated typed handler before its handler method is
        /// invoked.</param>
        /// <returns>The current transition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddEffect<TEffect>(ElementBuildAction<TEffect> buildAction = null)
            where TEffect : class, ITransitionEffect<TEvent>
            => AddEffect(async c => 
            {
                var state = await ((BaseContext)c).Context.Executor.GetTransitionEffectAsync<TEffect, TEvent>(c);
                buildAction?.Invoke(new ElementBuilder<TEffect>(state));
                await state.EffectAsync(c.Event);
            });

        /// <summary>Registers typed effects for the current transition.</summary>
        /// <typeparam name="TEffect1">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <typeparam name="TEffect2">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <returns>The current transition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddEffects<TEffect1, TEffect2>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
        {
            AddEffect<TEffect1>();
            return AddEffect<TEffect2>();
        }

        /// <summary>Registers typed effects for the current transition.</summary>
        /// <typeparam name="TEffect1">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <typeparam name="TEffect2">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <typeparam name="TEffect3">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <returns>The current transition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddEffects<TEffect1, TEffect2, TEffect3>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
            where TEffect3 : class, ITransitionEffect<TEvent>
        {
            AddEffects<TEffect1, TEffect2>();
            return AddEffect<TEffect3>();
        }

        /// <summary>Registers typed effects for the current transition.</summary>
        /// <typeparam name="TEffect1">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <typeparam name="TEffect2">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <typeparam name="TEffect3">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <typeparam name="TEffect4">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <returns>The current transition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddEffects<TEffect1, TEffect2, TEffect3, TEffect4>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
            where TEffect3 : class, ITransitionEffect<TEvent>
            where TEffect4 : class, ITransitionEffect<TEvent>
        {
            AddEffects<TEffect1, TEffect2, TEffect3>();
            return AddEffect<TEffect4>();
        }

        /// <summary>Registers typed effects for the current transition.</summary>
        /// <typeparam name="TEffect1">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <typeparam name="TEffect2">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <typeparam name="TEffect3">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <typeparam name="TEffect4">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <typeparam name="TEffect5">Typed transition effect implementing <see cref="ITransitionEffect&lt;TEvent&gt;" />.
        /// </typeparam>
        /// <returns>The current transition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddEffects<TEffect1, TEffect2, TEffect3, TEffect4, TEffect5>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
            where TEffect3 : class, ITransitionEffect<TEvent>
            where TEffect4 : class, ITransitionEffect<TEvent>
            where TEffect5 : class, ITransitionEffect<TEvent>
        {
            AddEffects<TEffect1, TEffect2, TEffect3, TEffect4>();
            return AddEffect<TEffect5>();
        }
    }
}
