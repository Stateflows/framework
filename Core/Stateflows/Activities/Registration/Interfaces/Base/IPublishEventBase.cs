namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IPublishEventBase<out TReturn>
    {
        TReturn AddPublishEventAction<TEvent>(string actionNodeName, PublishEventActionDelegateAsync<TEvent> actionAsync, PublishEventActionBuildAction buildAction = null);
    }
}
