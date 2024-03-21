﻿using System;
using System.Threading.Tasks;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Events;

namespace Stateflows.Common.StateMachines.Classes
{
    internal class StateMachineWrapper : IStateMachine
    {
        public BehaviorId Id => Behavior.Id;

        private IBehavior Behavior { get; }

        public StateMachineWrapper(IBehavior consumer)
        {
            Behavior = consumer;
        }

        public Task<RequestResult<CurrentStateResponse>> GetCurrentStateAsync()
            => RequestAsync(new CurrentStateRequest());

        public Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => Behavior.SendAsync(@event);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
            => Behavior.RequestAsync(request);

        public Task WatchAsync<TNotification>(Action<TNotification> handler)
            where TNotification : Notification, new()
            => Behavior.WatchAsync<TNotification>(handler);

        public Task UnwatchAsync<TNotification>()
            where TNotification : Notification, new()
            => Behavior.UnwatchAsync<TNotification>();

        public Task WatchCurrentStateAsync(Action<CurrentStateNotification> handler)
            => Behavior.WatchAsync(handler);

        public Task UnwatchCurrentStateAsync()
            => Behavior.UnwatchAsync<CurrentStateNotification>();
    }
}
