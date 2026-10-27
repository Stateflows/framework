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
    /// <summary>Registers callbacks awaited when a composite or orthogonal state's nested configuration is finalized.
    /// </summary>
    /// <typeparam name="TReturn">The fluent state builder returned after registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface ICompositeStateFinalization<out TReturn>
    {
        /// <summary>Registers handlers for the state's nested-state finalization operation.</summary>
        /// <param name="actionsAsync">Asynchronous handlers receiving the state action context during nested-state
        /// finalization. Each returned task is awaited.</param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddOnFinalize(params Func<IStateActionContext, Task>[] actionsAsync);
        
        #region AddOnFinalizeActivity
        /// <summary>Runs a new embedded Activity for each execution of the state's nested-state finalization operation.
        /// </summary>
        /// <typeparam name="TActivity">Embedded behavior implementation of <see cref="IActivity" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddOnFinalizeActivity<TActivity>()
            where TActivity : class, IActivity
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onFinalize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity<TActivity>(activityName), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnFinalize(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Finalization, c, activityName));
        }

        /// <summary>Runs a new embedded Activity for each execution of the state's nested-state finalization operation.
        /// </summary>
        /// <param name="activityBuildAction">Callback configuring the embedded Activity's nodes, flows, and lifecycle.
        /// </param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddOnFinalizeActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var activityName = $"{vertex.Graph.Name}.{vertex.Name}.onFinalize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActivities(b => b.AddActivity(activityName, activityBuildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnFinalize(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Finalization, c, activityName));
        }
        #endregion
        
        #region AddOnFinalizeAction
        /// <summary>Runs a new embedded Action for each execution of the state's nested-state finalization operation.
        /// </summary>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <typeparam name="TAction">Embedded behavior implementation of <see cref="IAction" />. Its definition is
        /// registered for this embedding site.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        [DebuggerHidden]
        public TReturn AddOnFinalizeAction<TAction>(ActionBuildAction<TAction> buildAction = null)
            where TAction : class, IAction
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onFinalize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction<TAction>(actionName, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnFinalize(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Finalization, c, actionName));
        }
        
        /// <summary>Runs a new embedded Action for each execution of the state's nested-state finalization operation.
        /// </summary>
        /// <param name="actionDelegateAsync">Asynchronous implementation receiving the embedded Action's execution context.
        /// </param>
        /// <param name="buildAction">Optional callback configuring the embedded Action's execution and middleware options.
        /// </param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behavioral-groups" />
        public TReturn AddOnFinalizeAction(ActionDelegateAsync actionDelegateAsync, ActionBuildAction buildAction = null)
        {
            var vertex = ((IVertexBuilder)this).Vertex;
            var actionName = $"{vertex.Graph.Name}.{vertex.Name}.onFinalize.{vertex.Entry.Actions.Count}";
            vertex.Graph.StateflowsBuilder.AddActions(b => b.AddAction(actionName, actionDelegateAsync, buildAction: buildAction), vertex.Graph.OwnerClass ?? vertex.Graph.Class, vertex.Graph.Class);
            return AddOnFinalize(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Finalization, c, actionName));
        }
        #endregion
        
        /// <summary>Registers handlers for the state's nested-state finalization operation.</summary>
        /// <param name="actions">Synchronous handlers receiving the state action context during nested-state finalization.
        /// </param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        public TReturn AddOnFinalize(params System.Action<IStateActionContext>[] actions)
            => AddOnFinalize(actions.Select(action => action.ToAsync()).ToArray());

        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TStateFinalization">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <param name="buildAction">Optional callback configuring the activated typed handler before its handler method is
        /// invoked.</param>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddOnFinalize<TStateFinalization>(ElementBuildAction<TStateFinalization> buildAction = null)
            where TStateFinalization : class, ICompositeStateFinalization
            => AddOnFinalize(async c => 
            {
                var state = await ((BaseContext)c).Context.Executor.GetStateAsync<TStateFinalization>(c);
                buildAction?.Invoke(new ElementBuilder<TStateFinalization>(state));
                await state.OnFinalizeAsync();
            });

        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TStateFinalization1">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TStateFinalization2">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddOnFinalize<TStateFinalization1, TStateFinalization2>()
            where TStateFinalization1 : class, ICompositeStateFinalization
            where TStateFinalization2 : class, ICompositeStateFinalization
        {
            AddOnFinalize<TStateFinalization1>();
            return AddOnFinalize<TStateFinalization2>();
        }

        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TStateFinalization1">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TStateFinalization2">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TStateFinalization3">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddOnFinalize<TStateFinalization1, TStateFinalization2, TStateFinalization3>()
            where TStateFinalization1 : class, ICompositeStateFinalization
            where TStateFinalization2 : class, ICompositeStateFinalization
            where TStateFinalization3 : class, ICompositeStateFinalization
        {
            AddOnFinalize<TStateFinalization1, TStateFinalization2>();
            return AddOnFinalize<TStateFinalization3>();
        }

        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TStateFinalization1">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TStateFinalization2">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TStateFinalization3">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TStateFinalization4">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddOnFinalize<TStateFinalization1, TStateFinalization2, TStateFinalization3, TStateFinalization4>()
            where TStateFinalization1 : class, ICompositeStateFinalization
            where TStateFinalization2 : class, ICompositeStateFinalization
            where TStateFinalization3 : class, ICompositeStateFinalization
            where TStateFinalization4 : class, ICompositeStateFinalization
        {
            AddOnFinalize<TStateFinalization1, TStateFinalization2, TStateFinalization3>();
            return AddOnFinalize<TStateFinalization4>();
        }

        /// <summary>Registers typed handlers for the state's nested-state finalization operation.</summary>
        /// <typeparam name="TStateFinalization1">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TStateFinalization2">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TStateFinalization3">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TStateFinalization4">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <typeparam name="TStateFinalization5">Typed nested-state finalization handler implementing
        /// <see cref="ICompositeStateFinalization" />.</typeparam>
        /// <returns>The current state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
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
