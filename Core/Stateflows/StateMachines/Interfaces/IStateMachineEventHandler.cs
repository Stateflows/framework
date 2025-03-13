using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineEventHandler
    {
        Type EventType { get; }

        Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
;
    }
}
