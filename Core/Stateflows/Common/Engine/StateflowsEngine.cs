﻿using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
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
    internal class StateflowsEngine : /*BackgroundService, */IHostedService
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

        private Task executionTask;

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
            @event.SentAt = DateTime.Now;

            var token = new EventHolder(id, @event, serviceProvider);

            EventQueue.Enqueue(token);

            return token;
        }

        [DebuggerHidden]
        public Task StartAsync(CancellationToken cancellationToken)
        {
            executionTask = Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    EventQueue.WaitAsync().GetAwaiter().GetResult();

                    var token = EventQueue.Dequeue();

                    if (token != null)
                    {
                        _ = Task.Run(() =>
                        {
                            token.Validation = token.Event.Validate();

                            var status = EventStatus.Invalid;

                            try
                            {
                                if (token.Validation.IsValid)
                                {
                                    status = ProcessEventAsync(token.TargetId, token.Event, token.ServiceProvider, token.Exceptions)
                                        .GetAwaiter()
                                        .GetResult();
                                }

                                token.Status = token.Validation.IsValid
                                    ? status
                                    : EventStatus.Invalid;

                                token.Handled.Set();
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    throw;
                                }
                                finally
                                {
                                    token.Status = EventStatus.Failed;

                                    token.Handled.Set();
                                }
                            }
                        });
                    }
                }
            });

            return Task.CompletedTask;
        }

        //[DebuggerHidden]
        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, TEvent @event, IServiceProvider serviceProvider, List<Exception> exceptions)
            where TEvent : Event, new()
        {
            var result = EventStatus.Undelivered;

            if (Processors.TryGetValue(id.Type, out var processor))
            {
                if (Interceptor.BeforeExecute(@event))
                {
                    TenantAccessor.CurrentTenantId = await TenantProvider.GetCurrentTenantIdAsync();

                    await using var lockHandle = await Lock.AquireLockAsync(id);

                    try
                    {
                        try
                        {
                            result = await processor.ProcessEventAsync(id, @event, serviceProvider, exceptions);
                        }
                        finally
                        {
                            var response = @event.GetResponse();
                            if (response != null)
                            {
                                response.SenderId = id;
                                response.SentAt = DateTime.Now;
                            }
                        }
                    }
                    finally
                    {
                        Interceptor.AfterExecute(@event);
                    }
                }
            }

            return result;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            executionTask.Wait();

            return Task.CompletedTask;
        }
    }
}
