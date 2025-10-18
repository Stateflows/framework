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
    public interface ICompositeStateFinalization<out TReturn>
    {
        /// <summary>
        /// Adds finalization handler to current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>async c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actionsAsync">Action handlers</param>
        TReturn AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync);
        
        #region AddOnFinalizeActivity
        /// <summary>
        /// Registers activity behavior that will be started when current state finalizes
        /// </summary>
       
        /// <typeparam name="TActivity">Activity behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddOnFinalizeActivity<TActivity>()
            where TActivity : class, IActivity
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onFinalize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName));
            return AddOnFinalize(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Finalization, c, activityName));
        }

        /// <summary>
        /// Registers activity behavior that will be started when current state finalizes
        /// </summary>
        /// <param name="activityBuildAction">Activity build action</param>
        public TReturn AddOnFinalizeActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onFinalize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction));
            return AddOnFinalize(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Finalization, c, activityName));
        }
        #endregion
        
        #region AddOnFinalizeAction
        /// <summary>
        /// Registers action behavior that will be started when current state finalizes
        /// </summary>
       
        /// <typeparam name="TAction">Action behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddOnFinalizeAction<TAction>()
            where TAction : class, IAction
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onFinalize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName));
            return AddOnFinalize(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Finalization, c, actionName));
        }
        
        /// <summary>
        /// Registers action behavior that will be started when current state finalizes
        /// </summary>
        /// <param name="actionDelegateAsync">Action delegate</param>
        /// <param name="reentrant">Determines if action can be reentrant</param>
        public TReturn AddOnFinalizeAction(ActionDelegateAsync actionDelegateAsync, bool reentrant = true)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onFinalize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegateAsync, reentrant));
            return AddOnFinalize(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Finalization, c, actionName));
        }
        #endregion
        
        /// <summary>
        /// Adds synchronous finalization handler coming from current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actions">Synchronous action handlers</param>
        [DebuggerHidden]
        public TReturn AddOnFinalize(params System.Action<IStateActionContext>[] actions)
            => AddOnFinalize(actions.Select(action => action.ToAsync()).ToArray());

        /// <summary>
        /// Adds multiple typed finalization handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateFinalization">The type of the first state finalization handler.</typeparam>
        TReturn AddOnFinalize<TStateFinalization>()
            where TStateFinalization : class, ICompositeStateFinalization
            => AddOnFinalize(async c => await (await ((BaseContext)c).Context.Executor.GetStateAsync<TStateFinalization>(c)).OnFinalizeAsync());

        /// <summary>
        /// Adds multiple typed finalization handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TStateFinalization2">The type of the second state finalization handler.</typeparam>
        TReturn AddOnFinalize<TStateFinalization1, TStateFinalization2>()
            where TStateFinalization1 : class, ICompositeStateFinalization
            where TStateFinalization2 : class, ICompositeStateFinalization
        {
            AddOnFinalize<TStateFinalization1>();
            return AddOnFinalize<TStateFinalization2>();
        }

        /// <summary>
        /// Adds multiple typed finalization handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TStateFinalization2">The type of the second state finalization handler.</typeparam>
        /// <typeparam name="TStateFinalization3">The type of the third state finalization handler.</typeparam>
        TReturn AddOnFinalize<TStateFinalization1, TStateFinalization2, TStateFinalization3>()
            where TStateFinalization1 : class, ICompositeStateFinalization
            where TStateFinalization2 : class, ICompositeStateFinalization
            where TStateFinalization3 : class, ICompositeStateFinalization
        {
            AddOnFinalize<TStateFinalization1, TStateFinalization2>();
            return AddOnFinalize<TStateFinalization3>();
        }

        /// <summary>
        /// Adds multiple typed finalization handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TStateFinalization2">The type of the second state finalization handler.</typeparam>
        /// <typeparam name="TStateFinalization3">The type of the third state finalization handler.</typeparam>
        /// <typeparam name="TStateFinalization4">The type of the fourth state finalization handler.</typeparam>
        TReturn AddOnFinalize<TStateFinalization1, TStateFinalization2, TStateFinalization3, TStateFinalization4>()
            where TStateFinalization1 : class, ICompositeStateFinalization
            where TStateFinalization2 : class, ICompositeStateFinalization
            where TStateFinalization3 : class, ICompositeStateFinalization
            where TStateFinalization4 : class, ICompositeStateFinalization
        {
            AddOnFinalize<TStateFinalization1, TStateFinalization2, TStateFinalization3>();
            return AddOnFinalize<TStateFinalization4>();
        }

        /// <summary>
        /// Adds multiple typed finalization handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateFinalization1">The type of the first state finalization handler.</typeparam>
        /// <typeparam name="TStateFinalization2">The type of the second state finalization handler.</typeparam>
        /// <typeparam name="TStateFinalization3">The type of the third state finalization handler.</typeparam>
        /// <typeparam name="TStateFinalization4">The type of the fourth state finalization handler.</typeparam>
        /// <typeparam name="TStateFinalization5">The type of the fifth state finalization handler.</typeparam>
        TReturn AddOnFinalize<TStateFinalization1, TStateFinalization2, TStateFinalization3, TStateFinalization4, TStateFinalization5>()
            where TStateFinalization1 : class, ICompositeStateFinalization
            where TStateFinalization2 : class, ICompositeStateFinalization
            where TStateFinalization3 : class, ICompositeStateFinalization
            where TStateFinalization4 : class, ICompositeStateFinalization
            where TStateFinalization5 : class, ICompositeStateFinalization
        {
            AddOnFinalize<TStateFinalization1, TStateFinalization2, TStateFinalization3, TStateFinalization4>();
            return AddOnFinalize<TStateFinalization5>();
        }
    }
}
