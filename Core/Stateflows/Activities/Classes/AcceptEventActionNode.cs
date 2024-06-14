using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class AcceptEventActionNode<TEvent> : BaseActionNode
        where TEvent : Event, new()
    {
        public new IAcceptEventActionContext<TEvent> Context
            => (IAcceptEventActionContext<TEvent>)base.Context;
    }
}
