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
    /// <summary>Registers actions awaited when the current state is exited.</summary>
    /// <typeparam name="TReturn">The fluent state builder returned after registration.</typeparam>
    /// <remarks>Internal transitions do not exit the state and therefore do not execute these callbacks.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    public interface IStateExit<out TReturn>
    {
        /// <summary>Registers handlers for the state's exit operation.</summary>
        /// <param name="actionsAsync">Asynchronous handlers receiving the state action context during exit. Each returned
        /// task is awaited.</param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddOnExit(params Func<IStateActionContext, Task>[] actionsAsync);

        #region AddOnExitActivity
        /// <summary>Runs a new embedded Activity for each execution of the state's exit operation.</summary>
        /// <param name="buildAction">Optional callback configuring the typed embedded Activity's utilities and middleware
        /// without redefining its graph.</param>
        /// <typeparam name="TActivity">Embedded behavior implementation of <see cref="IActivity" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddOnExitActivity<TActivity>(ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onExit.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnExit(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Exit, c, activityName));
        }

        /// <summary>Runs a new embedded Activity for each execution of the state's exit operation.</summary>
        /// <param name="activityBuildAction">Callback configuring the embedded Activity's nodes, flows, and lifecycle.
        /// </param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddOnExitActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onExit.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnExit(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Exit, c, activityName));
        }
        #endregion
        
        #region AddOnExitAction
        /// <summary>Runs a new embedded Action for each execution of the state's exit operation.</summary>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <typeparam name="TAction">Embedded behavior implementation of <see cref="IAction" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddOnExitAction<TAction>(ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onExit.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnExit(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Exit, c, actionName));
        }
        
        /// <summary>Runs a new embedded Action for each execution of the state's exit operation.</summary>
        /// <param name="actionDelegateAsync">Asynchronous implementation receiving the embedded Action's execution context.
        /// </param>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddOnExitAction(ActionDelegateAsync actionDelegateAsync, ActionBuildAction buildAction = null)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onExit.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegateAsync, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnExit(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Exit, c, actionName));
        }
        #endregion

        /// <summary>Registers handlers for the state's exit operation.</summary>
        /// <param name="actions">Synchronous handlers receiving the state action context during exit.</param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        [DebuggerHidden]
        public TReturn AddOnExit(params System.Action<IStateActionContext>[] actions)
            => AddOnExit(actions.Select(action => action.ToAsync()).ToArray());

        /// <summary>Registers typed handlers for the state's exit operation.</summary>
        /// <typeparam name="TStateExit">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <param name="buildAction">Optional callback configuring the activated typed handler before its handler method is
        /// invoked.</param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddOnExit<TStateExit>(ElementBuildAction<TStateExit> buildAction = null)
            where TStateExit : class, IStateExit
            => AddOnExit(async c => 
            {
                var state = await ((BaseContext)c).Context.Executor.GetStateAsync<TStateExit>(c);
                buildAction?.Invoke(new ElementBuilder<TStateExit>(state));
                await state.OnExitAsync();
            });

        /// <summary>Registers typed handlers for the state's exit operation.</summary>
        /// <typeparam name="TStateExit1">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <typeparam name="TStateExit2">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddOnExit<TStateExit1, TStateExit2>()
            where TStateExit1 : class, IStateExit
            where TStateExit2 : class, IStateExit
        {
            AddOnExit<TStateExit1>();
            return AddOnExit<TStateExit2>();
        }

        /// <summary>Registers typed handlers for the state's exit operation.</summary>
        /// <typeparam name="TStateExit1">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <typeparam name="TStateExit2">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <typeparam name="TStateExit3">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddOnExit<TStateExit1, TStateExit2, TStateExit3>()
            where TStateExit1 : class, IStateExit
            where TStateExit2 : class, IStateExit
            where TStateExit3 : class, IStateExit
        {
            AddOnExit<TStateExit1, TStateExit2>();
            return AddOnExit<TStateExit3>();
        }

        /// <summary>Registers typed handlers for the state's exit operation.</summary>
        /// <typeparam name="TStateExit1">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <typeparam name="TStateExit2">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <typeparam name="TStateExit3">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <typeparam name="TStateExit4">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddOnExit<TStateExit1, TStateExit2, TStateExit3, TStateExit4>()
            where TStateExit1 : class, IStateExit
            where TStateExit2 : class, IStateExit
            where TStateExit3 : class, IStateExit
            where TStateExit4 : class, IStateExit
        {
            AddOnExit<TStateExit1, TStateExit2, TStateExit3>();
            return AddOnExit<TStateExit4>();
        }

        /// <summary>Registers typed handlers for the state's exit operation.</summary>
        /// <typeparam name="TStateExit1">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <typeparam name="TStateExit2">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <typeparam name="TStateExit3">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <typeparam name="TStateExit4">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <typeparam name="TStateExit5">Typed state-exit handler implementing <see cref="IStateExit" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
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
