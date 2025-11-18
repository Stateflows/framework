using System;
using Stateflows.Common;

namespace Stateflows.Actions
{
    public interface IActionContextHolder : IAsyncDisposable
    {
        ActionId ActionId { get; }
        BehaviorStatus BehaviorStatus { get; }
        IBehaviorContext GetActionContext();
    }
}