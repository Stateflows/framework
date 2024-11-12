using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineEventHandler
    {
        Type EventType { get; }

        Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
;
    }
}
