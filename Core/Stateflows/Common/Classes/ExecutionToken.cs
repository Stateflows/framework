using Stateflows.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Stateflows.Common.Classes
{
    internal class ExecutionToken
    {
        public BehaviorId TargetId { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public EventHolder EventHolder { get; private set; }

        public EventWaitHandle Handled { get; } = new EventWaitHandle(false, EventResetMode.AutoReset);

        public EventStatus Status { get; set; }

        public List<Exception> Exceptions { get; set; } = new List<Exception>();

        public Dictionary<object, EventHolder> Responses { get; set; } = new Dictionary<object, EventHolder>();

        public EventValidation Validation { get; internal set; }

        public ExecutionToken(BehaviorId targetId, EventHolder eventHolder, IServiceProvider serviceProvider)
        {
            TargetId = targetId;
            EventHolder = eventHolder;
            ServiceProvider = serviceProvider;
        }
    }
}
