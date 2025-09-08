using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Activities.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ICompositeStateInitialization<out TReturn>
    {
        /// <summary>
        /// Adds initialization handler to current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>async c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actionsAsync">Action handlers</param>
        TReturn AddOnInitialize(params Func<IStateActionContext, Task>[] actionsAsync);
        
        /// <summary>
        /// Adds activity behavior that will be started when current state initializes
        /// </summary>
        /// <param name="activityName">Activity behavior name</param>
        /// <param name="buildAction">Build action</param>
        [DebuggerHidden]
        public TReturn AddOnInitializeActivity(string activityName, StateActionActivityBuildAction buildAction = null)
            => AddOnInitialize(c => StateMachineActivityExtensions.RunStateActivityAsync(Constants.Initialization, c, activityName, buildAction));

        /// <summary>
        /// Adds activity behavior that will be started when current state initializes
        /// </summary>
        /// <param name="buildAction">Build action</param>
        /// <typeparam name="TActivity">Activity behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddOnInitializeActivity<TActivity>(StateActionActivityBuildAction buildAction = null)
            where TActivity : class, IActivity
            => AddOnInitializeActivity(Activity<TActivity>.Name, buildAction);
        
        /// <summary>
        /// Adds action behavior that will be started when current state initializes
        /// </summary>
        /// <param name="actionName">Action behavior name</param>
        /// <param name="buildAction">Build action</param>
        [DebuggerHidden]
        public TReturn AddOnInitializeAction(string actionName, StateActionActionBuildAction buildAction = null)
            => AddOnInitialize(c => StateMachineActionExtensions.RunStateActionAsync(Constants.Initialization, c, actionName, buildAction));

        /// <summary>
        /// Adds action behavior that will be started when current state initializes
        /// </summary>
        /// <param name="buildAction">Build action</param>
        /// <typeparam name="TAction">Action behavior type</typeparam>
        [DebuggerHidden]
        public TReturn AddOnInitializeAction<TAction>(StateActionActionBuildAction buildAction = null)
            where TAction : class, IAction
            => AddOnInitializeAction(Stateflows.Actions.Action<TAction>.Name, buildAction);
        
        /// <summary>
        /// Adds synchronous initialization handler coming from current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actions">Synchronous action handlers</param>
        [DebuggerHidden]
        public TReturn AddOnInitialize(params System.Action<IStateActionContext>[] actions)
            => AddOnInitialize(
                actions.Select(action => action
                    .AddStateMachineInvocationContext(((IGraphBuilder)this).Graph)
                    .ToAsync()
                ).ToArray()
            );

        /// <summary>
        /// Adds multiple typed initialization handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateInitialization">The type of the first state initialization handler.</typeparam>
        TReturn AddOnInitialize<TStateInitialization>()
            where TStateInitialization : class, ICompositeStateInitialization
            => AddOnInitialize(async c => await (await ((BaseContext)c).Context.Executor.GetStateAsync<TStateInitialization>(c)).OnInitializeAsync());

        /// <summary>
        /// Adds multiple typed initialization handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TStateInitialization2">The type of the second state initialization handler.</typeparam>
        TReturn AddOnInitialize<TStateInitialization1, TStateInitialization2>()
            where TStateInitialization1 : class, ICompositeStateInitialization
            where TStateInitialization2 : class, ICompositeStateInitialization
        {
            AddOnInitialize<TStateInitialization1>();
            return AddOnInitialize<TStateInitialization2>();
        }

        /// <summary>
        /// Adds multiple typed initialization handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TStateInitialization2">The type of the second state initialization handler.</typeparam>
        /// <typeparam name="TStateInitialization3">The type of the third state initialization handler.</typeparam>
        TReturn AddOnInitialize<TStateInitialization1, TStateInitialization2, TStateInitialization3>()
            where TStateInitialization1 : class, ICompositeStateInitialization
            where TStateInitialization2 : class, ICompositeStateInitialization
            where TStateInitialization3 : class, ICompositeStateInitialization
        {
            AddOnInitialize<TStateInitialization1, TStateInitialization2>();
            return AddOnInitialize<TStateInitialization3>();
        }

        /// <summary>
        /// Adds multiple typed initialization handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TStateInitialization2">The type of the second state initialization handler.</typeparam>
        /// <typeparam name="TStateInitialization3">The type of the third state initialization handler.</typeparam>
        /// <typeparam name="TStateInitialization4">The type of the fourth state initialization handler.</typeparam>
        TReturn AddOnInitialize<TStateInitialization1, TStateInitialization2, TStateInitialization3, TStateInitialization4>()
            where TStateInitialization1 : class, ICompositeStateInitialization
            where TStateInitialization2 : class, ICompositeStateInitialization
            where TStateInitialization3 : class, ICompositeStateInitialization
            where TStateInitialization4 : class, ICompositeStateInitialization
        {
            AddOnInitialize<TStateInitialization1, TStateInitialization2, TStateInitialization3>();
            return AddOnInitialize<TStateInitialization4>();
        }

        /// <summary>
        /// Adds multiple typed initialization handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateInitialization1">The type of the first state initialization handler.</typeparam>
        /// <typeparam name="TStateInitialization2">The type of the second state initialization handler.</typeparam>
        /// <typeparam name="TStateInitialization3">The type of the third state initialization handler.</typeparam>
        /// <typeparam name="TStateInitialization4">The type of the fourth state initialization handler.</typeparam>
        /// <typeparam name="TStateInitialization5">The type of the fifth state initialization handler.</typeparam>
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
