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
    /// <summary>Registers callbacks awaited when a composite or orthogonal state's nested configuration is initialized.
    /// </summary>
    /// <typeparam name="TReturn">The fluent state builder returned after registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface ICompositeStateInitialization<out TReturn>
    {
        /// <summary>Registers handlers for the state's nested-state initialization operation.</summary>
        /// <param name="actionsAsync">Asynchronous handlers receiving the state action context during nested-state
        /// initialization. Each returned task is awaited.</param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync);
        
        #region AddOnInitializeActivity
        /// <summary>Runs a new embedded Activity for each execution of the state's nested-state initialization operation.
        /// </summary>
        /// <typeparam name="TActivity">Embedded behavior implementation of <see cref="IActivity" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddOnInitializeActivity<TActivity>()
            where TActivity : class, IActivity
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onInitialize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnInitialize(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Initialization, c, activityName));
        }

        /// <summary>Runs a new embedded Activity for each execution of the state's nested-state initialization operation.
        /// </summary>
        /// <param name="activityBuildAction">Callback configuring the embedded Activity's nodes, flows, and lifecycle.
        /// </param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddOnInitializeActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onInitialize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnInitialize(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Initialization, c, activityName));
        }
        #endregion
        
        #region AddOnInitializeAction
        /// <summary>Runs a new embedded Action for each execution of the state's nested-state initialization operation.
        /// </summary>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <typeparam name="TAction">Embedded behavior implementation of <see cref="IAction" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddOnInitializeAction<TAction>(ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onInitialize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnInitialize(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Initialization, c, actionName));
        }
        
        /// <summary>Runs a new embedded Action for each execution of the state's nested-state initialization operation.
        /// </summary>
        /// <param name="actionDelegateAsync">Asynchronous implementation receiving the embedded Action's execution context.
        /// </param>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddOnInitializeAction(ActionDelegateAsync actionDelegateAsync, ActionBuildAction buildAction = null)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onInitialize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegateAsync, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnInitialize(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Initialization, c, actionName));
        }
        #endregion
        
        /// <summary>Registers handlers for the state's nested-state initialization operation.</summary>
        /// <param name="actions">Synchronous handlers receiving the state action context during nested-state
        /// initialization.</param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        public TReturn AddOnInitialize(params System.Action<IStateActionContext>[] actions)
            => AddOnInitialize(actions.Select(action => action.ToAsync()).ToArray());

        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TStateInitialization">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <param name="buildAction">Optional callback configuring the activated typed handler before its handler method is
        /// invoked.</param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddOnInitialize<TStateInitialization>(ElementBuildAction<TStateInitialization> buildAction = null)
            where TStateInitialization : class, ICompositeStateInitialization
            => AddOnInitialize(async c => 
            {
                var state = await ((BaseContext)c).Context.Executor.GetStateAsync<TStateInitialization>(c);
                buildAction?.Invoke(new ElementBuilder<TStateInitialization>(state));
                await state.OnInitializeAsync();
            });

        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TStateInitialization1">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TStateInitialization2">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddOnInitialize<TStateInitialization1, TStateInitialization2>()
            where TStateInitialization1 : class, ICompositeStateInitialization
            where TStateInitialization2 : class, ICompositeStateInitialization
        {
            AddOnInitialize<TStateInitialization1>();
            return AddOnInitialize<TStateInitialization2>();
        }

        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TStateInitialization1">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TStateInitialization2">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TStateInitialization3">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddOnInitialize<TStateInitialization1, TStateInitialization2, TStateInitialization3>()
            where TStateInitialization1 : class, ICompositeStateInitialization
            where TStateInitialization2 : class, ICompositeStateInitialization
            where TStateInitialization3 : class, ICompositeStateInitialization
        {
            AddOnInitialize<TStateInitialization1, TStateInitialization2>();
            return AddOnInitialize<TStateInitialization3>();
        }

        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TStateInitialization1">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TStateInitialization2">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TStateInitialization3">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TStateInitialization4">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddOnInitialize<TStateInitialization1, TStateInitialization2, TStateInitialization3, TStateInitialization4>()
            where TStateInitialization1 : class, ICompositeStateInitialization
            where TStateInitialization2 : class, ICompositeStateInitialization
            where TStateInitialization3 : class, ICompositeStateInitialization
            where TStateInitialization4 : class, ICompositeStateInitialization
        {
            AddOnInitialize<TStateInitialization1, TStateInitialization2, TStateInitialization3>();
            return AddOnInitialize<TStateInitialization4>();
        }

        /// <summary>Registers typed handlers for the state's nested-state initialization operation.</summary>
        /// <typeparam name="TStateInitialization1">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TStateInitialization2">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TStateInitialization3">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TStateInitialization4">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <typeparam name="TStateInitialization5">Typed nested-state initialization handler implementing
        /// <see cref="ICompositeStateInitialization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddOnInitialize<TStateInitialization1, TStateInitialization2, TStateInitialization3, TStateInitialization4, TStateInitialization5>()
            where TStateInitialization1 : class, ICompositeStateInitialization
            where TStateInitialization2 : class, ICompositeStateInitialization
            where TStateInitialization3 : class, ICompositeStateInitialization
            where TStateInitialization4 : class, ICompositeStateInitialization
            where TStateInitialization5 : class, ICompositeStateInitialization
        {
            AddOnInitialize<TStateInitialization1, TStateInitialization2, TStateInitialization3, TStateInitialization4>();
            return AddOnInitialize<TStateInitialization5>();
        }
    }
}
