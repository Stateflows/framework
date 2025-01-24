using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
        /// <param name="actionAsync">Action handler</param>
        TReturn AddOnExit(Func<IStateActionContext, Task> actionAsync);

        /// <summary>
        /// Adds synchronous exit handler coming from current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="action">Synchronous action handler</param>
        [DebuggerHidden]
        public TReturn AddOnExit(Action<IStateActionContext> action)
            => AddOnExit(action
                .AddStateMachineInvocationContext(((IVertexBuilder)this).Vertex.Graph)
                .ToAsync()
            );

        /// <summary>
        /// Adds multiple typed exit handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateExit">The type of the state exit handler.</typeparam>
        TReturn AddOnExit<TStateExit>()
            where TStateExit : class, IStateExit
            => AddOnExit(c => ((BaseContext)c).Context.Executor.GetState<TStateExit>(c)?.OnExitAsync());

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
