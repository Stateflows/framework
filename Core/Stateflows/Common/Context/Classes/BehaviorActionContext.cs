using System;
using System.Collections.Generic;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Context.Classes
{
    internal class BehaviorActionContext(StateflowsContext context, IServiceProvider serviceProvider) :
        BaseContext(context, serviceProvider),
        IBehaviorActionContext
    {
        IBehaviorContext IBehaviorActionContext.Behavior => Behavior;

        // todo
        public object ExecutionTrigger { get; }
        public Guid ExecutionTriggerId { get; }
        public Dictionary<string, EventHeader> Headers { get; }
    }
}
