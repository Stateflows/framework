using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public abstract class SignalAction<TEvent> : ActivityNode
        where TEvent : Event
    {
        public abstract Task<TEvent> GenerateEventAsync();

        public abstract Task<BehaviorId> SelectTargetAsync();
    }

    public static class SignalActionInfo<TEvent, TSignalAction>
        where TEvent : Event
        where TSignalAction : SignalAction<TEvent>
    {
        public static string Name => typeof(TSignalAction).FullName;
    }
}
