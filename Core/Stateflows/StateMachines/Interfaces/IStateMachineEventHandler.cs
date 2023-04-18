using Stateflows.Common;
using Stateflows.StateMachines.Inspection.Interfaces;
using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IStateMachineEventHandler
    {
        string EventName { get; }

        Task<bool> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
            where TEvent : Event, new();
    }
}
