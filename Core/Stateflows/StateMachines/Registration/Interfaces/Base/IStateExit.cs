using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using ActionDelegateAsync = Stateflows.Actions.Registration.ActionDelegateAsync;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateExit<out TReturn>
    {
        /// <summary>
        /// Adds exit handler to current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>async c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actionsAsync">Action handlers</param>
        TReturn AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync);

        #region AddOnExitActivity
        /// <summary>
        /// Registers activity behavior that will be started when current state exits
        /// </summary>
        /// <param name="buildAction">Build action</param>
        /// <typeparam name="TActivity">Activity behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddOnExitActivity<TActivity>()
            where TActivity : class, IActivity
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onExit.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName));
            return AddOnExit(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Exit, c, activityName));
        }

        /// <summary>
        /// Registers activity behavior that will be started when current state enters
        /// </summary>
        /// <param name="activityBuildAction">Activity build action</param>
        public TReturn AddOnExitActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onExit.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction));
            return AddOnExit(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Exit, c, activityName));
        }
        #endregion
        
        #region AddOnExitAction
        /// <summary>
        /// Registers action behavior that will be started when current state exits
        /// </summary>
        /// <param name="buildAction">Build action</param>
        /// <typeparam name="TAction">Action behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddOnExitAction<TAction>()
            where TAction : class, IAction
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onExit.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName));
            return AddOnExit(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Exit, c, actionName));
        }
        
        /// <summary>
        /// Registers action behavior that will be started when current state exits
        /// </summary>
        /// <param name="actionDelegateAsync">Action delegate</param>
        /// <param name="reentrant">Determines if action can be reentrant</param>
        public TReturn AddOnExitAction(ActionDelegateAsync actionDelegateAsync, bool reentrant = true)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onExit.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegateAsync, reentrant));
            return AddOnExit(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Exit, c, actionName));
        }
        #endregion

        /// <summary>
        /// Adds synchronous exit handler coming from current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actions">Synchronous action handlers</param>
        [DebuggerHidden]
        public TReturn AddOnExit(params System.Action<IStateActionContext>[] actions)
            => AddOnExit(actions.Select(action => action.ToAsync()).ToArray());

        /// <summary>
        /// Adds multiple typed exit handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateExit">The type of the state exit handler.</typeparam>
        TReturn AddOnExit<TStateExit>()
            where TStateExit : class, IStateExit
            => AddOnExit(async c => await (await ((BaseContext)c).Context.Executor.GetStateAsync<TStateExit>(c)).OnExitAsync());

        /// <summary>
        /// Adds multiple typed exit handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateExit1">The type of the first state exit handler.</typeparam>
        /// <typeparam name="TStateExit2">The type of the second state exit handler.</typeparam>
        TReturn AddOnExit<TStateExit1, TStateExit2>()
            where TStateExit1 : class, IStateExit
            where TStateExit2 : class, IStateExit
        {
            AddOnExit<TStateExit1>();
            return AddOnExit<TStateExit2>();
        }

        /// <summary>
        /// Adds multiple typed exit handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateExit1">The type of the first state exit handler.</typeparam>
        /// <typeparam name="TStateExit2">The type of the second state exit handler.</typeparam>
        /// <typeparam name="TStateExit3">The type of the third state exit handler.</typeparam>
        TReturn AddOnExit<TStateExit1, TStateExit2, TStateExit3>()
            where TStateExit1 : class, IStateExit
            where TStateExit2 : class, IStateExit
            where TStateExit3 : class, IStateExit
        {
            AddOnExit<TStateExit1, TStateExit2>();
            return AddOnExit<TStateExit3>();
        }

        /// <summary>
        /// Adds multiple typed exit handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateExit1">The type of the first state exit handler.</typeparam>
        /// <typeparam name="TStateExit2">The type of the second state exit handler.</typeparam>
        /// <typeparam name="TStateExit3">The type of the third state exit handler.</typeparam>
        /// <typeparam name="TStateExit4">The type of the fourth state exit handler.</typeparam>
        TReturn AddOnExit<TStateExit1, TStateExit2, TStateExit3, TStateExit4>()
            where TStateExit1 : class, IStateExit
            where TStateExit2 : class, IStateExit
            where TStateExit3 : class, IStateExit
            where TStateExit4 : class, IStateExit
        {
            AddOnExit<TStateExit1, TStateExit2, TStateExit3>();
            return AddOnExit<TStateExit4>();
        }

        /// <summary>
        /// Adds multiple typed exit handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateExit1">The type of the first state exit handler.</typeparam>
        /// <typeparam name="TStateExit2">The type of the second state exit handler.</typeparam>
        /// <typeparam name="TStateExit3">The type of the third state exit handler.</typeparam>
        /// <typeparam name="TStateExit4">The type of the fourth state exit handler.</typeparam>
        /// <typeparam name="TStateExit5">The type of the fifth state exit handler.</typeparam>
        TReturn AddOnExit<TStateExit1, TStateExit2, TStateExit3, TStateExit4, TStateExit5>()
            where TStateExit1 : class, IStateExit
            where TStateExit2 : class, IStateExit
            where TStateExit3 : class, IStateExit
            where TStateExit4 : class, IStateExit
            where TStateExit5 : class, IStateExit
        {
            AddOnExit<TStateExit1, TStateExit2, TStateExit3, TStateExit4>();
            return AddOnExit<TStateExit5>();
        }
    }
}
