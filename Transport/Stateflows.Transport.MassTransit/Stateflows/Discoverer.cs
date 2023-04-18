using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MassTransit;
using Stateflows.Transport.Common.Interfaces;
using Stateflows.Transport.MassTransit.MassTransit.Messages;

namespace Stateflows.Transport.MassTransit.MassTransit
{
    internal class Discoverer : IBehaviorClassesDiscoverer
    {
        public string PreflightId = Guid.NewGuid().ToString();

        private IPublishEndpoint PublishEndpoint { get; }

        public Discoverer(IBus bus)
        {
            PublishEndpoint = bus;
        }

        public Task DiscoverBehaviorClassesAsync(IEnumerable<BehaviorClass> localBehaviorClasses)
        {
            var request = new PreflightRequest()
            {
                RequestId = PreflightId,
                AvailableBehaviorClasses = localBehaviorClasses
            };

            return PublishEndpoint.Publish<PreflightRequest>(request);
        }
    }
}
