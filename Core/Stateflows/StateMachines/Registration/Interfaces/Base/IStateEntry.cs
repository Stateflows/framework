using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Builders;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using ActionBuildAction = Stateflows.Actions.Registration.Interfaces.ActionBuildAction;
using ActionDelegateAsync = Stateflows.Actions.Registration.ActionDelegateAsync;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateEntry<out TReturn>
    {
        /// <summary>
        /// Adds entry handler to current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>async c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actionsAsync">Action handlers</param>
        TReturn AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync);

        #region AddOnEntryActivity
        /// <summary>
        /// Registers activity behavior that will be started when current state enters
        /// </summary>
       
        /// <typeparam name="TActivity">Activity behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddOnEntryActivity<TActivity>(ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onEntry.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnEntry(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Entry, c, activityName));
        }

        /// <summary>
        /// Registers activity behavior that will be started when current state enters
        /// </summary>
        /// <param name="activityBuildAction">Activity build action</param>
        public TReturn AddOnEntryActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onEntry.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnEntry(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Entry, c, activityName));
        }
        #endregion
        
        #region AddOnEntryAction
        /// <summary>
        /// Registers action behavior that will be started when current state enters
        /// </summary>
        /// <typeparam name="TAction">Action behavior type</typeparam>
        /// <param name="buildAction">Build action</param>
        [DebuggerHidden]
        public TReturn AddOnEntryAction<TAction>(ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onEntry.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName, buildAction: buildAction, version: 1), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnEntry(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Entry, c, actionName));
        }
        
        /// <summary>
        /// Registers action behavior that will be started when current state enters
        /// </summary>
        /// <param name="actionDelegateAsync">Action delegate</param>
        /// <param name="buildAction">Build action</param>
        public TReturn AddOnEntryAction(ActionDelegateAsync actionDelegateAsync, ActionBuildAction buildAction = null)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onEntry.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegateAsync, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnEntry(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Entry, c, actionName));
        }
        #endregion
        
        /// <summary>
        /// Adds synchronous entry handler coming from current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actions">Synchronous action handlers</param>
        [DebuggerHidden]
        public TReturn AddOnEntry(params System.Action<IStateActionContext>[] actions)
            => AddOnEntry(actions.Select(action => action.ToAsync()).ToArray());

        /// <summary>
        /// Adds multiple typed entry handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateEntry">The type of the first state entry handler.</typeparam>
        TReturn AddOnEntry<TStateEntry>(ElementBuildAction<TStateEntry> buildAction = null)
            where TStateEntry : class, IStateEntry
            => AddOnEntry(async c =>
            {
                var state = await ((BaseContext)c).Context.Executor.GetStateAsync<TStateEntry>(c);
                buildAction?.Invoke(new ElementBuilder<TStateEntry>(state));
                await state.OnEntryAsync();
            });

        /// <summary>
        /// Adds multiple typed entry handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateEntry1">The type of the first state entry handler.</typeparam>
        /// <typeparam name="TStateEntry2">The type of the second state entry handler.</typeparam>
        TReturn AddOnEntry<TStateEntry1, TStateEntry2>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
        {
            AddOnEntry<TStateEntry1>();
            return AddOnEntry<TStateEntry2>();
        }

        /// <summary>
        /// Adds multiple typed entry handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateEntry1">The type of the first state entry handler.</typeparam>
        /// <typeparam name="TStateEntry2">The type of the second state entry handler.</typeparam>
        /// <typeparam name="TStateEntry3">The type of the third state entry handler.</typeparam>
        TReturn AddOnEntry<TStateEntry1, TStateEntry2, TStateEntry3>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
            where TStateEntry3 : class, IStateEntry
        {
            AddOnEntry<TStateEntry1, TStateEntry2>();
            return AddOnEntry<TStateEntry3>();
        }

        /// <summary>
        /// Adds multiple typed entry handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateEntry1">The type of the first state entry handler.</typeparam>
        /// <typeparam name="TStateEntry2">The type of the second state entry handler.</typeparam>
        /// <typeparam name="TStateEntry3">The type of the third state entry handler.</typeparam>
        /// <typeparam name="TStateEntry4">The type of the fourth state entry handler.</typeparam>
        TReturn AddOnEntry<TStateEntry1, TStateEntry2, TStateEntry3, TStateEntry4>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
            where TStateEntry3 : class, IStateEntry
            where TStateEntry4 : class, IStateEntry
        {
            AddOnEntry<TStateEntry1, TStateEntry2, TStateEntry3>();
            return AddOnEntry<TStateEntry4>();
        }

        /// <summary>
        /// Adds multiple typed entry handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateEntry1">The type of the first state entry handler.</typeparam>
        /// <typeparam name="TStateEntry2">The type of the second state entry handler.</typeparam>
        /// <typeparam name="TStateEntry3">The type of the third state entry handler.</typeparam>
        /// <typeparam name="TStateEntry4">The type of the fourth state entry handler.</typeparam>
        /// <typeparam name="TStateEntry5">The type of the fifth state entry handler.</typeparam>
        TReturn AddOnEntry<TStateEntry1, TStateEntry2, TStateEntry3, TStateEntry4, TStateEntry5>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
            where TStateEntry3 : class, IStateEntry
            where TStateEntry4 : class, IStateEntry
            where TStateEntry5 : class, IStateEntry
        {
            AddOnEntry<TStateEntry1, TStateEntry2, TStateEntry3, TStateEntry4>();
            return AddOnEntry<TStateEntry5>();
        }
    }
}
