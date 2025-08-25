using System;

namespace Stateflows.Common.Enums
{
    [Flags]
    public enum StorageKind
    {
        Context = 1,
        Notifications = 2,
        All = Context | Notifications,
    }
}