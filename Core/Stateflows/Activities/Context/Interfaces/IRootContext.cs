using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Engine;

namespace Stateflows.Activities.Context.Interfaces
{
    internal interface IRootContext
    {
        NodeScope NodeScope { get; }

        RootContext Context { get; }
    }
}
