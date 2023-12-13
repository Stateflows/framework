﻿using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IAcceptEvent<out TReturn>
    {
        TReturn AddAcceptEventAction<TEvent>(string actionNodeName, AcceptEventActionDelegateAsync<TEvent> eventActionAsync, AcceptEventActionBuilderAction buildAction = null)
            where TEvent : Event, new();

        //TReturn AddEventAction<TEvent, TEventAction>(EventActionBuilderAction buildAction = null)
        //    where TEvent : Event
        //    where TEventAction : EventAction<TEvent>;

        //TReturn AddEventAction<TEvent, TEventAction>(string actionNodeName, EventActionBuilderAction buildAction = null)
        //    where TEvent : Event
        //    where TEventAction : EventAction<TEvent>;
    }
}
