using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Entities.Context.Classes
{
    internal sealed class EntityEventContext<TEvent>(EntityActionContext actionContext, EventHolder<TEvent> eventHolder)
        : DelegatingActionContext(actionContext), IEventContext<TEvent>
    {
        public TEvent Event { get; } = eventHolder.Payload;

        public Guid EventId { get; } = eventHolder.Id;

        public override object ExecutionTrigger => Event!;

        public override Guid ExecutionTriggerId => EventId;

        public override IDictionary<string, EventHeader> Headers { get; } = eventHolder.Headers;
    }
}