using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Engine;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    internal class StateflowsEngine : BackgroundService, IHostedService
    {
        private IServiceScope Scope { get; }
        private IServiceProvider ServiceProvider { get; }
        private IStateflowsLock Lock { get; }
        private IStateflowsExecutionInterceptor Interceptor { get; }

        private Dictionary<string, IEventProcessor> processors;
        private Dictionary<string, IEventProcessor> Processors
            => processors ??= ServiceProvider.GetRequiredService<IEnumerable<IEventProcessor>>().ToDictionary(p => p.BehaviorType, p => p);

        private EventQueue<EventHolder> EventQueue { get; } = new EventQueue<EventHolder>(true);

        public StateflowsEngine(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();
            ServiceProvider = Scope.ServiceProvider;
            Lock = ServiceProvider.GetRequiredService<IStateflowsLock>();
            Interceptor = ServiceProvider.GetRequiredService<CommonInterceptor>();
        }

        public EventHolder EnqueueEvent(BehaviorId id, Event @event, IServiceProvider serviceProvider)
        {
            var token = new EventHolder(id, @event, serviceProvider);

            EventQueue.Enqueue(token);

            return token;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, TEvent @event, IServiceProvider serviceProvider)
            where TEvent : Event, new()
        {
            var result = EventStatus.Undelivered;
            try
            {

            if (Processors.TryGetValue(id.Type, out var processor))
            {
                try
                {
                    Interceptor.BeforeExecute(@event);

                    try
                    {
                        await Lock.LockAsync(id);

                        result = await processor.ProcessEventAsync(id, @event, serviceProvider);
                    }
                    finally
                    {
                        await Lock.UnlockAsync(id);
                    }
                }
                finally
                {
                    Interceptor.AfterExecute(@event);
                }
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return result;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await EventQueue.WaitAsync();

                var token = EventQueue.Dequeue();

                if (token != null)
                {
                    _ = Task.Run(async () =>
                    {
                        token.Validation = token.Event.Validate();

                        token.Status = token.Validation.IsValid
                            ? await ProcessEventAsync(token.TargetId, token.Event, token.ServiceProvider)
                            : EventStatus.Invalid;

                        token.Handled.Set();
                    });
                }
            }
        }
    }
}
