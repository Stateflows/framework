using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface ISendEvent<out TReturn>
    {
        TReturn AddSendEventAction<TEvent>(string actionNodeName, SendEventActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SendEventActionBuilderAction buildAction = null)
            where TEvent : Event, new();
    }
}
