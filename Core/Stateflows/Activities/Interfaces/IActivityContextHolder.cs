using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public interface IActivityContextHolder : IAsyncDisposable
    {
        ActivityId ActivityId { get; }
        BehaviorStatus BehaviorStatus { get; }
        // IEnumerable<string> ExpectedEventNames { get; }
        Task<IEnumerable<string>> GetExpectedEventNamesAsync();
        IReadOnlyTree<string> ActiveNodes { get; }
        IActivityContext GetActivityContext();
        IActivityNodeContext GetNodeContext(string nodeName);
    }
}