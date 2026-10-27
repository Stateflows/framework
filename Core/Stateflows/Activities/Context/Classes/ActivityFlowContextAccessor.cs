using System.Threading;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    /// <summary>Provides ambient access to the flow context in the current asynchronous execution flow.</summary>
    /// <remarks>The context is valid only for the corresponding flow callback and should not be retained after it completes.</remarks>
    public static class ActivityFlowContextAccessor
    {
        /// <summary>Gets the flow context currently active in this asynchronous flow, if any.</summary>
        public static readonly AsyncLocal<IActivityFlowContext> Context = new AsyncLocal<IActivityFlowContext>();
    }
}
