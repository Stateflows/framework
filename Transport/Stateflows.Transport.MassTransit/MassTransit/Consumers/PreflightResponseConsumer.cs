using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Stateflows.Transport.MassTransit.Stateflows;
using Stateflows.Transport.MassTransit.MassTransit.Messages;

namespace Stateflows.Transport.MassTransit.MassTransit.Consumers
{
    internal class PreflightResponseConsumer : IConsumer<PreflightResponse>
    {
        private BehaviorProvider behaviorProvider;
        private BehaviorProvider BehaviorProvider => behaviorProvider ?? (behaviorProvider = ServiceProvider.GetService<BehaviorProvider>());

        private IServiceProvider ServiceProvider { get; set; }

        Discoverer discoverer;
        Discoverer Discoverer => discoverer ?? (discoverer = ServiceProvider.GetService<Discoverer>());

        public PreflightResponseConsumer(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public Task Consume(ConsumeContext<PreflightResponse> context)
        {
            return context.Message != null && context.Message.RequestId == Discoverer.PreflightId
                ? BehaviorProvider.AddBehaviorClasses(context.Message.AvailableBehaviorClasses)
                : Task.CompletedTask;
        }
    }
}
