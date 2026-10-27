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
    /// <summary>Registers actions awaited when the current state is entered, including embedded one-shot behaviors.
    /// </summary>
    /// <typeparam name="TReturn">The fluent state builder returned after registration.</typeparam>
    /// <remarks>Entry and initialization are distinct operations: initialization establishes nested state
    /// configuration.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    public interface IStateEntry<out TReturn>
    {
        /// <summary>Registers handlers for the state's entry operation.</summary>
        /// <param name="actionsAsync">Asynchronous handlers receiving the state action context during entry. Each returned
        /// task is awaited.</param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddOnEntry(params Func<IStateActionContext, Task>[] actionsAsync);

        #region AddOnEntryActivity
        /// <summary>Runs a new embedded Activity for each execution of the state's entry operation.</summary>
        /// <param name="buildAction">Optional callback configuring the typed embedded Activity's utilities and middleware
        /// without redefining its graph.</param>
        /// <typeparam name="TActivity">Embedded behavior implementation of <see cref="IActivity" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddOnEntryActivity<TActivity>(ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onEntry.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnEntry(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Entry, c, activityName));
        }

        /// <summary>Runs a new embedded Activity for each execution of the state's entry operation.</summary>
        /// <param name="activityBuildAction">Callback configuring the embedded Activity's nodes, flows, and lifecycle.
        /// </param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddOnEntryActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onEntry.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnEntry(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Entry, c, activityName));
        }
        #endregion
        
        #region AddOnEntryAction
        /// <summary>Runs a new embedded Action for each execution of the state's entry operation.</summary>
        /// <typeparam name="TAction">Embedded behavior implementation of <see cref="IAction" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddOnEntryAction<TAction>(ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onEntry.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName, buildAction: buildAction, version: 1), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnEntry(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Entry, c, actionName));
        }
        
        /// <summary>Runs a new embedded Action for each execution of the state's entry operation.</summary>
        /// <param name="actionDelegateAsync">Asynchronous implementation receiving the embedded Action's execution context.
        /// </param>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddOnEntryAction(ActionDelegateAsync actionDelegateAsync, ActionBuildAction buildAction = null)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onEntry.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegateAsync, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnEntry(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Entry, c, actionName));
        }
        #endregion
        
        /// <summary>Registers handlers for the state's entry operation.</summary>
        /// <param name="actions">Synchronous handlers receiving the state action context during entry.</param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        [DebuggerHidden]
        public TReturn AddOnEntry(params System.Action<IStateActionContext>[] actions)
            => AddOnEntry(actions.Select(action => action.ToAsync()).ToArray());

        /// <summary>Registers typed handlers for the state's entry operation.</summary>
        /// <typeparam name="TStateEntry">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <param name="buildAction">Optional callback configuring the activated typed handler before its handler method is
        /// invoked.</param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddOnEntry<TStateEntry>(ElementBuildAction<TStateEntry> buildAction = null)
            where TStateEntry : class, IStateEntry
            => AddOnEntry(async c =>
            {
                var state = await ((BaseContext)c).Context.Executor.GetStateAsync<TStateEntry>(c);
                buildAction?.Invoke(new ElementBuilder<TStateEntry>(state));
                await state.OnEntryAsync();
            });

        /// <summary>Registers typed handlers for the state's entry operation.</summary>
        /// <typeparam name="TStateEntry1">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <typeparam name="TStateEntry2">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddOnEntry<TStateEntry1, TStateEntry2>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
        {
            AddOnEntry<TStateEntry1>();
            return AddOnEntry<TStateEntry2>();
        }

        /// <summary>Registers typed handlers for the state's entry operation.</summary>
        /// <typeparam name="TStateEntry1">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <typeparam name="TStateEntry2">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <typeparam name="TStateEntry3">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddOnEntry<TStateEntry1, TStateEntry2, TStateEntry3>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
            where TStateEntry3 : class, IStateEntry
        {
            AddOnEntry<TStateEntry1, TStateEntry2>();
            return AddOnEntry<TStateEntry3>();
        }

        /// <summary>Registers typed handlers for the state's entry operation.</summary>
        /// <typeparam name="TStateEntry1">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <typeparam name="TStateEntry2">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <typeparam name="TStateEntry3">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <typeparam name="TStateEntry4">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddOnEntry<TStateEntry1, TStateEntry2, TStateEntry3, TStateEntry4>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
            where TStateEntry3 : class, IStateEntry
            where TStateEntry4 : class, IStateEntry
        {
            AddOnEntry<TStateEntry1, TStateEntry2, TStateEntry3>();
            return AddOnEntry<TStateEntry4>();
        }

        /// <summary>Registers typed handlers for the state's entry operation.</summary>
        /// <typeparam name="TStateEntry1">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <typeparam name="TStateEntry2">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <typeparam name="TStateEntry3">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <typeparam name="TStateEntry4">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <typeparam name="TStateEntry5">Typed state-entry handler implementing <see cref="IStateEntry" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
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
