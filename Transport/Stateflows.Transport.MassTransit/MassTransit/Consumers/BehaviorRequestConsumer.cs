using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MassTransit;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using StateflowsEvent = Stateflows.Common.Event;
using Stateflows.Transport.MassTransit.MassTransit.Messages;

namespace Stateflows.Transport.MassTransit.MassTransit.Consumers
{
    internal class BehaviorRequestConsumer : IConsumer<BehaviorRequest>
    {
        private IEnumerable<IBehaviorProvider> Providers { get; }

        public BehaviorRequestConsumer(IEnumerable<IBehaviorProvider> providers)
        {
            Providers = providers;
        }

        public async Task Consume(ConsumeContext<BehaviorRequest> context)
        {
            StateflowsEvent @event = StateflowsJsonConverter.DeserializeObject(context.Message.RequestData) as StateflowsEvent;

            var providers = Providers
                .Where(p =>
                    p.IsLocal &&
                    p.BehaviorClasses.Contains(context.Message.BehaviorId.BehaviorClass)
                );

            IBehavior behavior = null;
            foreach (var provider in providers)
            {
                if (provider.TryProvideBehavior(context.Message.BehaviorId, out behavior))
                {
                    break;
                }
            }

            if (behavior != null)
            {
                var responseMessage = new BehaviorResponse() { BehaviorId = context.Message.BehaviorId };

                var result = await behavior.SendAsync(@event);
                responseMessage.Status = result.Status;

                if (@event.IsRequest())
                {
                    var response = @event.GetResponse();
                    responseMessage.ResponseData = StateflowsJsonConverter.SerializePolymorphicObject(response);
                }

                if (result.Validation != null)
                {
                    responseMessage.ValidationData = StateflowsJsonConverter.SerializePolymorphicObject(result.Validation);
                }

                context.Respond(responseMessage);
            }
        }
    }
}
