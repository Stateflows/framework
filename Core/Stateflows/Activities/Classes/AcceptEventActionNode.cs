using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class AcceptEventActionNode<TEvent> : ActionNode
        where TEvent : Event, new()
    {
        public new IAcceptEventActionContext<TEvent> Context { get; internal set; }
    }
}
