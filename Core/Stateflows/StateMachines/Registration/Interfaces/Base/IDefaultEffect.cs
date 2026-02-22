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

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IDefaultEffect<out TReturn>
    {
        /// <summary>
        /// Adds an asynchronous effect to the current transition.<br/>
        /// Use the following pattern to implement the effect:
        /// <code>async c => {
        ///     // effect logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="effectsAsync">Asynchronous effect handlers</param>
        TReturn AddEffect(params Func<ITransitionContext<Completion>, Task>[] effectsAsync);

        TReturn AddEffect(Delegate effectDelegate)
            => AddEffect(c => effectDelegate.InvokeDelegateActionAsync(
                StateflowsActivator.ResolveParameterValueFactories(
                    c.Behavior.ServiceProvider,
                    null,
                    "transition effect",
                    effectDelegate.Method.GetParameters()
                )
            ));

        /// <summary>
        /// Registers Activity behavior as effect
        /// </summary>
        /// <typeparam name="TActivity">Activity behavior type</typeparam>
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

        /// <summary>
        /// Registers Activity behavior as effect
        /// </summary>
        /// <param name="activityBuildAction">Activity build action</param>
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
        
        /// <summary>
        /// Registers action behavior as effect
        /// </summary>
        /// <typeparam name="TAction">Action behavior type</typeparam>
        /// <param name="buildAction">Build action</param>
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

        /// <summary>
        /// Registers Action behavior as effect
        /// </summary>
        /// <param name="actionDelegate">Action delegate</param>
        /// <param name="buildAction">Build action</param>
        public TReturn AddEffectAction(Stateflows.Actions.Registration.ActionDelegateAsync actionDelegate, Stateflows.Actions.Registration.Interfaces.ActionBuildAction buildAction = null)
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
        
        /// <summary>
        /// Adds a synchronous effect to the current transition.<br/>
        /// Use the following pattern to implement the effect:
        /// <code>c => {
        ///     // effect logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="effects">Synchronous effect handlers</param>
        [DebuggerHidden]
        public TReturn AddEffect(params System.Action<ITransitionContext<Completion>>[] effects)
            => AddEffect(effects.Select(effect => effect.ToAsync()).ToArray());

        /// <summary>
        /// Adds a typed effect to the current transition.
        /// </summary>
        /// <typeparam name="TEffect">The type of the effect handler.</typeparam>
        TReturn AddEffect<TEffect>(ElementBuildAction<TEffect> buildAction = null)
            where TEffect : class, IDefaultTransitionEffect
            => AddEffect(async c => 
            {
                var state = await ((BaseContext)c).Context.Executor.GetDefaultTransitionEffectAsync<TEffect>(c);
                buildAction?.Invoke(new ElementBuilder<TEffect>(state));
                await state.EffectAsync();
            });

        /// <summary>
        /// Adds multiple typed effects to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2>()
            where TEffect1 : class, IDefaultTransitionEffect
            where TEffect2 : class, IDefaultTransitionEffect
        {
            AddEffect<TEffect1>();
            return AddEffect<TEffect2>();
        }

        /// <summary>
        /// Adds multiple typed effects to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2, TEffect3>()
            where TEffect1 : class, IDefaultTransitionEffect
            where TEffect2 : class, IDefaultTransitionEffect
            where TEffect3 : class, IDefaultTransitionEffect
        {
            AddEffects<TEffect1, TEffect2>();
            return AddEffect<TEffect3>();
        }

        /// <summary>
        /// Adds multiple typed effects to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        /// <typeparam name="TEffect4">The type of the fourth effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2, TEffect3, TEffect4>()
            where TEffect1 : class, IDefaultTransitionEffect
            where TEffect2 : class, IDefaultTransitionEffect
            where TEffect3 : class, IDefaultTransitionEffect
            where TEffect4 : class, IDefaultTransitionEffect
        {
            AddEffects<TEffect1, TEffect2, TEffect3>();
            return AddEffect<TEffect4>();
        }

        /// <summary>
        /// Adds multiple typed effects to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        /// <typeparam name="TEffect4">The type of the fourth effect handler.</typeparam>
        /// <typeparam name="TEffect5">The type of the fifth effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2, TEffect3, TEffect4, TEffect5>()
            where TEffect1 : class, IDefaultTransitionEffect
            where TEffect2 : class, IDefaultTransitionEffect
            where TEffect3 : class, IDefaultTransitionEffect
            where TEffect4 : class, IDefaultTransitionEffect
            where TEffect5 : class, IDefaultTransitionEffect
        {
            AddEffects<TEffect1, TEffect2, TEffect3, TEffect4>();
            return AddEffect<TEffect5>();
        }
    }
}
