using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines;

namespace Stateflows.Common.StateMachines.Classes
{
    internal class StateMachineWrapper : IStateMachine
    {
        BehaviorId IBehavior.Id => Behavior.Id;

        private IBehavior Behavior { get; }

        public StateMachineWrapper(IBehavior consumer)
        {
            Behavior = consumer;
        }

        public async Task<CurrentStateResponse> GetCurrentStateAsync()
            => (await RequestAsync(new CurrentStateRequest())).Response;

        public Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event
            => Behavior.SendAsync(@event);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response
            => Behavior.RequestAsync(request);
    }
}
