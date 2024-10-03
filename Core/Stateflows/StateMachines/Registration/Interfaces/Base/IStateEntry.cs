using System;
using System.Threading.Tasks;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateEntry<TReturn>
    {
        /// <summary>
        /// Adds entry handler to current state.<br/>
        /// Use the following pattern to implement handler:
        /// <code>async c => {
        ///     // handler logic here; action context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="actionAsync">Action handler</param>
        TReturn AddOnEntry(Func<IStateActionContext, Task> actionAsync);

        TReturn AddOnEntry<TStateEntry>()
            where TStateEntry : class, IStateEntry
        {
            (this as IInternal).Services.AddServiceType<TStateEntry>();

            return AddOnEntry(c => (c as BaseContext).Context.Executor.GetState<TStateEntry>(c)?.OnEntryAsync());
        }

        TReturn AddOnEntries<TStateEntry1, TStateEntry2>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
        {
            AddOnEntry<TStateEntry1>();
            return AddOnEntry<TStateEntry2>();
        }

        TReturn AddOnEntries<TStateEntry1, TStateEntry2, TStateEntry3>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
            where TStateEntry3 : class, IStateEntry
        {
            AddOnEntries<TStateEntry1, TStateEntry2>();
            return AddOnEntry<TStateEntry3>();
        }

        TReturn AddOnEntries<TStateEntry1, TStateEntry2, TStateEntry3, TStateEntry4>()
            where TStateEntry1 : class, IStateEntry
            where TStateEntry2 : class, IStateEntry
            where TStateEntry3 : class, IStateEntry
            where TStateEntry4 : class, IStateEntry
        {
            AddOnEntries<TStateEntry1, TStateEntry2, TStateEntry3>();
            return AddOnEntry<TStateEntry4>();
        }

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
