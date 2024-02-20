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

        public Task<RequestResult<SubscriptionResponse>> SubscribeAsync<TEvent>(BehaviorId behaviorId) where TEvent : Event, new()
        {
            throw new global::System.NotImplementedException();
        }

        public Task<RequestResult<UnsubscriptionResponse>> UnsubscribeAsync<TEvent>(BehaviorId behaviorId) where TEvent : Event, new()
        {
            throw new global::System.NotImplementedException();
        }
    }
}
