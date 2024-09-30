using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsEngine
    {
        Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions, Dictionary<object, EventHolder> responses);
    }
}
