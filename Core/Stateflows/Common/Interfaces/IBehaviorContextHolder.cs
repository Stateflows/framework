using System;

namespace Stateflows.Common
{
    public interface IBehaviorContextHolder : IAsyncDisposable
    {
        BehaviorId BehaviorId { get; }
        BehaviorStatus BehaviorStatus { get; }
        IBehaviorContext GetBehaviorContext();
    }
}