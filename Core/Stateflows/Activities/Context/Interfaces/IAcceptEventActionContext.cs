using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IAcceptEventActionContext<out TEvent> : IActionContext
    {
        TEvent Event { get; }
    }
}
