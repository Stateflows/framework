using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IAcceptEvent<out TReturn>
    {
        TReturn AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuildAction buildAction = null)
            where TEvent : Event, new();

        TReturn AddTimeEventAction<TTimeEvent>(string actionNodeName, TimeEventActionDelegateAsync eventActionAsync, AcceptEventActionBuildAction buildAction = null)
            where TTimeEvent : TimeEvent, new();
    }
}
