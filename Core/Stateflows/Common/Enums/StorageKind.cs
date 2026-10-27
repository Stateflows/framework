using System;

namespace Stateflows.Common.Enums
{
    /// <summary>Flags selecting the categories of Stateflows data persisted by a storage integration.</summary>
    [Flags]
    public enum StorageKind
    {
        /// <summary>Behavior execution contexts.</summary>
        Context = 1,
        /// <summary>Retained notifications.</summary>
        Notifications = 2,
        /// <summary>Behavior values.</summary>
        Values = 4,
        /// <summary>All supported persisted data categories.</summary>
        All = Context | Notifications | Values,
    }
}