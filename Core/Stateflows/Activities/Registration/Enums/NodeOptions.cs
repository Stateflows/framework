using System;

namespace Stateflows.Activities
{
    [Flags]
    public enum NodeOptions
    {
        None = 0,
        ImplicitJoin = 1,
        ImplicitFork = 2,
        Reentrant = 4,
        Default = ImplicitJoin | ImplicitFork,
    }
}
