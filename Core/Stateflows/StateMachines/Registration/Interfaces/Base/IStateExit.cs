using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

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

        /// <summary>
        /// Adds activity behavior that will be started when current state exits
        /// </summary>
        /// <param name="activityName">Name of activity behavior</param>
        /// <param name="buildAction">Build action</param>
        [DebuggerHidden]
        public TReturn AddOnExitActivity(string activityName, StateActionActivityBuildAction buildAction = null)
            => AddOnExit(c => StateMachineActivityExtensions.RunStateActivity(Constants.Exit, c, activityName, buildAction));

        /// <summary>
        /// Adds action behavior that will be started when current state exits
        /// </summary>
        /// <param name="buildAction">Build action</param>
        /// <typeparam name="TAction">Action behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddOnExitAction<TAction>(StateActionActionBuildAction buildAction = null)
            where TAction : class, IAction
            => AddOnExitAction(Stateflows.Actions.Action<TAction>.Name, buildAction);

        /// <summary>
        /// Adds action behavior that will be started when current state exits
        /// </summary>
        /// <param name="actionName">Name of action behavior</param>
        /// <param name="buildAction">Build action</param>
        [DebuggerHidden]
        public TReturn AddOnExitAction(string actionName, StateActionActionBuildAction buildAction = null)
            => AddOnExit(c => StateMachineActionExtensions.RunStateAction(Constants.Exit, c, actionName, buildAction));

        /// <summary>
        /// Adds activity behavior that will be started when current state exits
        /// </summary>
        /// <param name="buildAction">Build action</param>
        /// <typeparam name="TActivity">Activity behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddOnExitActivity<TActivity>(StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnExitActivity(Activity<TActivity>.Name, buildAction);

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
            => AddOnExit(
                actions.Select(action => action
                    .AddStateMachineInvocationContext(((IGraphBuilder)this).Graph)
                    .ToAsync()
                ).ToArray()
            );

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
        TReturn AddOnExits<TStateExit1, TStateExit2>()
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
        TReturn AddOnExits<TStateExit1, TStateExit2, TStateExit3>()
            where TStateExit1 : class, IStateExit
            where TStateExit2 : class, IStateExit
            where TStateExit3 : class, IStateExit
        {
            AddOnExits<TStateExit1, TStateExit2>();
            return AddOnExit<TStateExit3>();
        }

        /// <summary>
        /// Adds multiple typed exit handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateExit1">The type of the first state exit handler.</typeparam>
        /// <typeparam name="TStateExit2">The type of the second state exit handler.</typeparam>
        /// <typeparam name="TStateExit3">The type of the third state exit handler.</typeparam>
        /// <typeparam name="TStateExit4">The type of the fourth state exit handler.</typeparam>
        TReturn AddOnExits<TStateExit1, TStateExit2, TStateExit3, TStateExit4>()
            where TStateExit1 : class, IStateExit
            where TStateExit2 : class, IStateExit
            where TStateExit3 : class, IStateExit
            where TStateExit4 : class, IStateExit
        {
            AddOnExits<TStateExit1, TStateExit2, TStateExit3>();
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
        TReturn AddOnExits<TStateExit1, TStateExit2, TStateExit3, TStateExit4, TStateExit5>()
            where TStateExit1 : class, IStateExit
            where TStateExit2 : class, IStateExit
            where TStateExit3 : class, IStateExit
            where TStateExit4 : class, IStateExit
            where TStateExit5 : class, IStateExit
        {
            AddOnExits<TStateExit1, TStateExit2, TStateExit3, TStateExit4>();
            return AddOnExit<TStateExit5>();
        }
    }
}
