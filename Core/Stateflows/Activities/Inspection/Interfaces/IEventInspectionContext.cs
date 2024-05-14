using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface IEventInspectionContext<out TEvent> : IEventContext<TEvent>
        where TEvent : Event, new()
    {
        new IActivityInspectionContext Activity { get; }
    }
}
