using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    internal interface IEventProcessor
    {
        string BehaviorType { get; }

        Task<EventStatus> ProcessEventAsync(BehaviorId id, EventHolder eventHolder, IServiceProvider serviceProvider, List<Exception> exceptions);
    }
}
