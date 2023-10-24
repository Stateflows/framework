using System.Threading.Tasks;
using MassTransit;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Event = Stateflows.Common.Event;
using Response = Stateflows.Common.Response;
using Stateflows.Transport.MassTransit.MassTransit.Messages;
using Stateflows.Common.Classes;

namespace Stateflows.Transport.MassTransit.Stateflows
{
    internal class Behavior : IBehavior
    {
        private IBus Bus { get; }

        public BehaviorId Id { get; }

        public Behavior(IBus bus, BehaviorId id)
        {
            Bus = bus;
            Id = id;
        }

        //public async Task<bool> SendAsync<TEvent>(TEvent @event)
        //    where TEvent : Event
        //{
        //    var result = await Bus.Request<BehaviorRequest, BehaviorResponse>(
        //        new BehaviorRequest()
        //        {
        //            BehaviorId = Id,
        //            RequestData = StateflowsJsonConverter.SerializeObject(@event)
        //        }
        //    );

        //    if (!string.IsNullOrEmpty(result.Message.ResponseData))
        //    {
        //        @event.Respond(StateflowsJsonConverter.DeserializeObject(result.Message.ResponseData) as Response);
        //    }

        //    if (!string.IsNullOrEmpty(result.Message.ValidationData))
        //    {
        //        var validation = StateflowsJsonConverter.DeserializeObject<EventValidation>(result.Message.ValidationData);

        //        @event.SetValidation(validation);
        //    }

        //    return result.Message.Consumed;
        //}

        //public async Task<TResponse> RequestAsync<TResponse>(Request<TResponse> request)
        //    where TResponse : Response
        //{
        //    _ = await SendAsync(request as Event);
        //    return request.Response;
        //}

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event
        {
            var result = await Bus.Request<BehaviorRequest, BehaviorResponse>(
                new BehaviorRequest()
                {
                    BehaviorId = Id,
                    RequestData = StateflowsJsonConverter.SerializeObject(@event)
                }
            );

            if (!string.IsNullOrEmpty(result.Message.ResponseData))
            {
                @event.Respond(StateflowsJsonConverter.DeserializeObject(result.Message.ResponseData) as Response);
            }

            EventValidation validation = null;
            if (!string.IsNullOrEmpty(result.Message.ValidationData))
            {
                validation = StateflowsJsonConverter.DeserializeObject<EventValidation>(result.Message.ValidationData);
            }

            return new SendResult(result.Message.Status, validation);
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response
        {
            var result = await SendAsync(request as Event);

            return new RequestResult<TResponse>(result.Status, result.Validation, request.Response);
        }
    }
}
