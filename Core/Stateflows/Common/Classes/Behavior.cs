using System.Threading.Tasks;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    internal class Behavior : IBehavior
    {
        public BehaviorId Id { get; }

        private StateflowsEngine Engine { get; set; }

        public Behavior(StateflowsEngine engine, BehaviorId id)
        {
            Id = id;
            Engine = engine;
        }

        public async Task<bool> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var holder = Engine.EnqueueEvent(Id, @event);
            await holder.Handled.WaitOneAsync();
            return holder.Consumed;
        }

        public async Task<TResponse> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            var holder = Engine.EnqueueEvent(Id, request);
            await holder.Handled.WaitOneAsync();
            return request.Response;
        }
    }
}
