using System;
using System.Threading;

namespace Stateflows.Common.Classes
{
    internal class EventHolder
    {
        public BehaviorId TargetId { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public object Event { get; private set; }

        public EventWaitHandle Handled { get; } = new EventWaitHandle(false, EventResetMode.AutoReset);

        public EventStatus Status { get; set; }

        public EventValidation Validation { get; internal set; }

        public EventHolder(BehaviorId targetId, object @event, IServiceProvider serviceProvider)
        {
            TargetId = targetId;
            Event = @event;
            ServiceProvider = serviceProvider;
        }
    }
}
