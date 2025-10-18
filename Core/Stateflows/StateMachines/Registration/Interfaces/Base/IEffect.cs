using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using ActionDelegateAsync = Stateflows.Actions.Registration.ActionDelegateAsync;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IEffect<TEvent, out TReturn>
    {
        /// <summary>
        /// Adds an asynchronous effect function to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>async c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="effectsAsync">Asynchronous effect functions</param>
        TReturn AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync);

        /// <summary>
        /// Registers Activity behavior as effect
        /// </summary>
        /// <typeparam name="TActivity">Activity behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddEffectActivity<TActivity>()
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
            
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName));
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
            
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction));
            return AddEffect(c => StateMachineActivityExtensions.RunEffectActivity(c, activityName));
        }
        
        /// <summary>
        /// Registers action behavior as effect
        /// </summary>
        /// <typeparam name="TAction">Action behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddEffectAction<TAction>()
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
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName));
            return AddEffect(c => StateMachineActionExtensions.RunEffectActionAsync(c, actionName));
        }

        /// <summary>
        /// Registers Action behavior as effect
        /// </summary>
        /// <param name="actionDelegate">Action delegate</param>
        /// <param name="reentrant">Flag that determines if action delegate can be executed in parallel</param>
        public TReturn AddEffectAction(ActionDelegateAsync actionDelegate, bool reentrant = true)
        {
            var edge = ((IEdgeBuilder)this).Edge;
            var vertex = edge.Source;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.{edge.Trigger}";
            if (edge.Target != null)
            {
                actionName += $".{edge.Target}";
            }
            actionName += $".effect.{edge.Effects.Actions.Count}";
            
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegate, reentrant));
            return AddEffect(c => StateMachineActionExtensions.RunEffectActionAsync(c, actionName));
        }
        
        /// <summary>
        /// Adds a synchronous effect function to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="effects">Synchronous effect functions</param>
        [DebuggerHidden]
        public TReturn AddEffect(params System.Action<ITransitionContext<TEvent>>[] effects)
            => AddEffect(effects.Select(effect => effect.ToAsync()).ToArray());

        /// <summary>
        /// Adds a typed effect handler to the current transition.
        /// </summary>
        /// <typeparam name="TEffect">The type of the effect handler.</typeparam>
        TReturn AddEffect<TEffect>()
            where TEffect : class, ITransitionEffect<TEvent>
            => AddEffect(async c => await (await ((BaseContext)c).Context.Executor.GetTransitionEffectAsync<TEffect, TEvent>(c)).EffectAsync(c.Event));

        /// <summary>
        /// Adds multiple typed effect handlers to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
        {
            AddEffect<TEffect1>();
            return AddEffect<TEffect2>();
        }

        /// <summary>
        /// Adds multiple typed effect handlers to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2, TEffect3>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
            where TEffect3 : class, ITransitionEffect<TEvent>
        {
            AddEffects<TEffect1, TEffect2>();
            return AddEffect<TEffect3>();
        }

        /// <summary>
        /// Adds multiple typed effect handlers to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        /// <typeparam name="TEffect4">The type of the fourth effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2, TEffect3, TEffect4>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
            where TEffect3 : class, ITransitionEffect<TEvent>
            where TEffect4 : class, ITransitionEffect<TEvent>
        {
            AddEffects<TEffect1, TEffect2, TEffect3>();
            return AddEffect<TEffect4>();
        }

        /// <summary>
        /// Adds multiple typed effect handlers to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        /// <typeparam name="TEffect4">The type of the fourth effect handler.</typeparam>
        /// <typeparam name="TEffect5">The type of the fifth effect handler.</typeparam>
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
