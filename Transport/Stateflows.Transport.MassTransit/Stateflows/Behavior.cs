using System.Threading.Tasks;
using MassTransit;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Event = Stateflows.Common.Event;
using Response = Stateflows.Common.Response;
using Stateflows.Transport.MassTransit.MassTransit.Messages;

namespace Stateflows.Transport.MassTransit.Stateflows
{
    internal class Behavior : IBehavior
    {
        private IBus Bus { get; }

        private BehaviorId Id { get; }

        public Behavior(IBus bus, BehaviorId id)
        {
            Bus = bus;
            Id = id;
        }

        public async Task<bool> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var result = await Bus.Request<BehaviorRequest, BehaviorResponse>(
                new BehaviorRequest()
                {
                    BehaviorId = Id,
                    RequestData = StateflowsJsonConverter.SerializeObject(@event)
                }
            );

            return result.Message.Consumed;
        }

        public async Task<TResponse> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            var result = await Bus.Request<BehaviorRequest, BehaviorResponse>(
                new BehaviorRequest()
                {
                    BehaviorId = Id,
                    RequestData = StateflowsJsonConverter.SerializeObject(request)
                }
            );

            return StateflowsJsonConverter.DeserializeObject(result.Message.ResponseData) as TResponse;
        }
    }
}
