using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface ISendEvent<out TReturn>
    {
        TReturn AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuildAction buildAction = null)
            where TEvent : Event, new();
    }
}
