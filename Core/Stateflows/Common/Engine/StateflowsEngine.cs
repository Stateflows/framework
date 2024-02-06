using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Engine;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    internal class StateflowsEngine : BackgroundService, IHostedService
    {
        private readonly IServiceScope Scope;
        private IServiceProvider ServiceProvider => Scope.ServiceProvider;
        private readonly IStateflowsLock Lock;
        private readonly IStateflowsExecutionInterceptor Interceptor;
        private readonly IStateflowsTenantProvider TenantProvider;
        private readonly ITenantAccessor TenantAccessor;
        private readonly ILogger<StateflowsEngine> Logger;
        private Dictionary<string, IEventProcessor> processors;
        private Dictionary<string, IEventProcessor> Processors
            => processors ??= ServiceProvider.GetRequiredService<IEnumerable<IEventProcessor>>().ToDictionary(p => p.BehaviorType, p => p);

        private EventQueue<EventHolder> EventQueue { get; } = new EventQueue<EventHolder>(true);

        public StateflowsEngine(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();
            Lock = ServiceProvider.GetRequiredService<IStateflowsLock>();
            Interceptor = ServiceProvider.GetRequiredService<CommonInterceptor>();
            TenantAccessor = ServiceProvider.GetRequiredService<ITenantAccessor>();
            TenantProvider = ServiceProvider.GetRequiredService<IStateflowsTenantProvider>();
            Logger = ServiceProvider.GetRequiredService<ILogger<StateflowsEngine>>();
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

            if (Processors.TryGetValue(id.Type, out var processor))
            {
                try
                {
                    if (Interceptor.BeforeExecute(@event))
                    {
                        TenantAccessor.CurrentTenantId = await TenantProvider.GetCurrentTenantIdAsync();

                        await using var lockHandle = await Lock.AquireLockAsync(id);

                        try
                        {
                            try
                            {
                                result = await processor.ProcessEventAsync(id, @event, serviceProvider);
                            }
                            finally
                            {
                                var response = @event.GetResponse();
                                if (response != null)
                                {
                                    response.SenderId = id;
                                }
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
                    Logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateflowsEngine).FullName, nameof(ProcessEventAsync), e.GetType().Name, e.Message);
                }
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
