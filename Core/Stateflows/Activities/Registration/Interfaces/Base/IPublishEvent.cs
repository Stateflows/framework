﻿using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IPublishEvent<out TReturn>
    {
        TReturn AddPublishEventAction<TEvent>(string actionNodeName, PublishEventActionDelegateAsync<TEvent> actionAsync, PublishEventActionBuilderAction buildAction = null)
            where TEvent : Event, new();
    }
}
