using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsEngine
    {
        Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions, Dictionary<object, EventHolder> responses);
    }
}
