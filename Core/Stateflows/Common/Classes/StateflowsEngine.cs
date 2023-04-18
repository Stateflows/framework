using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    internal class StateflowsEngine : BackgroundService, IHostedService
    {
        public IStateflowsLock Lock { get; }
        public Dictionary<string, IEventProcessor> Processors { get; } = new Dictionary<string, IEventProcessor>();
        private EventQueue<EventHolder> EventQueue { get; } = new EventQueue<EventHolder>(true);

        public EventHolder EnqueueEvent(BehaviorId id, Event @event)
        {
            var token = new EventHolder(id, @event);

            EventQueue.Enqueue(token);

            return token;
        }

        public StateflowsEngine(IStateflowsLock @lock, IEnumerable<IEventProcessor> processors)
        {
            Lock = @lock;
            Processors = processors.ToDictionary(p => p.BehaviorType, p => p);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public async Task<bool> ProcessEventAsync<TEvent>(BehaviorId id, TEvent @event)
            where TEvent : Event, new()
        {
            var result = false;

            if (Processors.TryGetValue(id.Type, out var processor))
            {
                await Lock.Lock(id);

                result = await processor.ProcessEventAsync(id, @event);

                await Lock.Unlock(id);
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
                        token.Consumed = await ProcessEventAsync(token.TargetId, token.Event);
                        token.Handled.Set();
                    });
                }
            }
        }
    }
}
