using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Interfaces;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public abstract class AcceptEventActionNode<TEvent> : BaseActionNode, IAcceptEventActionNode<TEvent>
        where TEvent : Event, new()
    {
        public new IAcceptEventActionContext<TEvent> Context
            => (IAcceptEventActionContext<TEvent>)base.Context;

        public abstract Task ExecuteAsync(TEvent @event);
    }
}
