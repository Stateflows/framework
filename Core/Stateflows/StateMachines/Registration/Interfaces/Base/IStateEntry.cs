using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

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
        
        /// <summary>
        /// Adds synchronous entry handler coming from current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actions">Synchronous action handlers</param>
        [DebuggerHidden]
        public TReturn AddOnEntry(params Action<IStateActionContext>[] actions)
            => AddOnEntry(
                actions.Select(action => action
                    .AddStateMachineInvocationContext(((IVertexBuilder)this).Vertex.Graph)
                    .ToAsync()
                ).ToArray()
            );

        /// <summary>
        /// Adds multiple typed entry handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateEntry">The type of the first state entry handler.</typeparam>
        TReturn AddOnEntry<TStateEntry>()
            where TStateEntry : class, IStateEntry
            => AddOnEntry(c => ((BaseContext)c).Context.Executor.GetState<TStateEntry>(c)?.OnEntryAsync());

        /// <summary>
        /// Adds multiple typed entry handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateEntry1">The type of the first state entry handler.</typeparam>
        /// <typeparam name="TStateEntry2">The type of the second state entry handler.</typeparam>
        TReturn AddOnEntries<TStateEntry1, TStateEntry2>()
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
        TReturn AddOnEntries<TStateEntry1, TStateEntry2, TStateEntry3>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
            where TStateEntry3 : class, IStateEntry
        {
            AddOnEntries<TStateEntry1, TStateEntry2>();
            return AddOnEntry<TStateEntry3>();
        }

        /// <summary>
        /// Adds multiple typed entry handlers to the current state.
        /// </summary>
        /// <typeparam name="TStateEntry1">The type of the first state entry handler.</typeparam>
        /// <typeparam name="TStateEntry2">The type of the second state entry handler.</typeparam>
        /// <typeparam name="TStateEntry3">The type of the third state entry handler.</typeparam>
        /// <typeparam name="TStateEntry4">The type of the fourth state entry handler.</typeparam>
        TReturn AddOnEntries<TStateEntry1, TStateEntry2, TStateEntry3, TStateEntry4>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
            where TStateEntry3 : class, IStateEntry
            where TStateEntry4 : class, IStateEntry
        {
            AddOnEntries<TStateEntry1, TStateEntry2, TStateEntry3>();
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
        TReturn AddOnEntries<TStateEntry1, TStateEntry2, TStateEntry3, TStateEntry4, TStateEntry5>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
            where TStateEntry3 : class, IStateEntry
            where TStateEntry4 : class, IStateEntry
            where TStateEntry5 : class, IStateEntry
        {
            AddOnEntries<TStateEntry1, TStateEntry2, TStateEntry3, TStateEntry4>();
            return AddOnEntry<TStateEntry5>();
        }
    }
}
