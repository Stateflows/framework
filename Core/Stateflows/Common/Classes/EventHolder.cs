﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Stateflows.Common.Classes
{
    internal class EventHolder
    {
        public BehaviorId TargetId { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public Event Event { get; private set; }

        public EventWaitHandle Handled { get; } = new EventWaitHandle(false, EventResetMode.AutoReset);

        public EventStatus Status { get; set; }

        public List<Exception> Exceptions { get; set; } = new List<Exception>();

        public EventValidation Validation { get; internal set; }

        public EventHolder(BehaviorId targetId, Event @event, IServiceProvider serviceProvider)
        {
            TargetId = targetId;
            Event = @event;
            ServiceProvider = serviceProvider;
        }
    }
}
