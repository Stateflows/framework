using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MassTransit;
using Stateflows.Common.Interfaces;
using Stateflows.Transport.MassTransit.MassTransit.Messages;

namespace Stateflows.Transport.MassTransit.MassTransit.Consumers
{
    internal class PreflightRequestConsumer : IConsumer<PreflightRequest>
    {
        private Discoverer Discoverer { get; }

        private IBehaviorClassesProvider ClassesProvider { get; }

        private IPublishEndpoint PublishEndpoint { get; }

        public PreflightRequestConsumer(IBehaviorClassesProvider classesProvider, Discoverer discoverer, IBus bus)
        {
            ClassesProvider = classesProvider;
            Discoverer = discoverer;
            PublishEndpoint = bus;
        }

        public Task Consume(ConsumeContext<PreflightRequest> context)
        {
            return context.Message.RequestId != Discoverer.PreflightId
                ? PublishEndpoint.Publish(
                    new PreflightResponse()
                    {
                        RequestId = context.Message.RequestId,
                        SenderId = Discoverer.PreflightId,
                        AvailableBehaviorClasses = ClassesProvider.LocalBehaviorClasses,
                    }
                )
                : Task.CompletedTask;
        }
    }
}
