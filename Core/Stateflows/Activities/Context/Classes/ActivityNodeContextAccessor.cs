using System.Threading;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    /// <summary>Provides ambient access to the node context in the current asynchronous execution flow.</summary>
    /// <remarks>The context is valid only for the corresponding node callback and should not be retained after it completes.</remarks>
    public static class ActivityNodeContextAccessor
    {
        /// <summary>Gets the node context currently active in this asynchronous flow, if any.</summary>
        public static readonly AsyncLocal<IActivityNodeContext> Context = new AsyncLocal<IActivityNodeContext>();
    }
}
