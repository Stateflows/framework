using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public abstract class SendEventAction<TEvent> : ActivityNode
        where TEvent : Event, new()
    {
        public abstract Task<TEvent> GenerateEventAsync();

        public abstract Task<BehaviorId> SelectTargetAsync();
    }
}
