using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines;

namespace Stateflows.Common.StateMachines.Classes
{
    internal class StateMachineWrapper : IStateMachine
    {
        private IBehavior Behavior { get; }

        public StateMachineWrapper(IBehavior consumer)
        {
            Behavior = consumer;
        }

        public async Task<bool> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => await Behavior.SendAsync(@event);

        public async Task<TResponse> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
            => await Behavior.RequestAsync(request);

        public async Task<StateDescriptor> GetCurrentStateAsync()
            => (await RequestAsync(new CurrentStateRequest()))?.CurrentState ?? null;

        public async Task<IEnumerable<string>> GetExpectedEventsAsync()
            => (await RequestAsync(new ExpectedEventsRequest()))?.ExpectedEvents ?? new List<string>();
    }
}
