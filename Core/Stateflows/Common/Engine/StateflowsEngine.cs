using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Engine;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using System.Reflection;

namespace Stateflows.Common
{
    internal class StateflowsEngine : IHostedService, IStateflowsEngine
    {
        private readonly IServiceScope Scope;
        private IServiceProvider ServiceProvider => Scope.ServiceProvider;
        private readonly IStateflowsLock Lock;
        private readonly IStateflowsExecutionInterceptor Interceptor;
        private readonly IStateflowsTenantProvider TenantProvider;
        private readonly ITenantAccessor TenantAccessor;
        private Dictionary<string, IEventProcessor> processors;
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        private Dictionary<string, IEventProcessor> Processors
            => processors ??= ServiceProvider.GetRequiredService<IEnumerable<IEventProcessor>>().ToDictionary(p => p.BehaviorType, p => p);

        private EventQueue<ExecutionToken> EventQueue { get; } = new EventQueue<ExecutionToken>(true);

        private Task executionTask;

        public StateflowsEngine(IServiceProvider serviceProvider)
        {
            Scope = serviceProvider.CreateScope();
            Lock = ServiceProvider.GetRequiredService<IStateflowsLock>();
            Interceptor = ServiceProvider.GetRequiredService<CommonInterceptor>();
            TenantAccessor = ServiceProvider.GetRequiredService<ITenantAccessor>();
            TenantProvider = ServiceProvider.GetRequiredService<IStateflowsTenantProvider>();
        }

        public ExecutionToken EnqueueEvent(BehaviorId id, EventHolder eventHolder, IServiceProvider serviceProvider)
        {
            var token = new ExecutionToken(id, eventHolder, serviceProvider);

            EventQueue.Enqueue(token);

            return token;
        }

        [DebuggerHidden]
        public Task StartAsync(CancellationToken cancellationToken)
        {
            executionTask = Task.Run(() =>
            {
                while (!CancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (!EventQueue.WaitAsync(CancellationTokenSource.Token).GetAwaiter().GetResult())
                    {
                        continue;
                    }

                    var token = EventQueue.Dequeue();

                    if (token != null)
                    {
                        _ = Task.Run(() =>
                        {
                            ResponseHolder.SetResponses(token.Responses);

                            token.Validation = token.EventHolder.Validate();

                            ResponseHolder.ClearResponses();

                            var status = EventStatus.Invalid;

                            try
                            {
                                if (token.Validation.IsValid)
                                {
                                    status = token.EventHolder.ProcessEventAsync(this, token.TargetId, token.Exceptions, token.Responses)
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
        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions, Dictionary<object, EventHolder> responses)
        {
            var result = EventStatus.Undelivered;

            if (Processors.TryGetValue(id.Type, out var processor) && Interceptor.BeforeExecute(eventHolder))
            {
                TenantAccessor.CurrentTenantId = await TenantProvider.GetCurrentTenantIdAsync().ConfigureAwait(false);

                await using var lockHandle = await Lock.AquireLockAsync(id).ConfigureAwait(false);

                try
                {
                    try
                    {
                        ResponseHolder.SetResponses(responses);

                        result = await processor.ProcessEventAsync(id, eventHolder, exceptions).ConfigureAwait(false);
                    }
                    finally
                    {
                        var responseHolder = eventHolder.GetResponseHolder();
                        if (responseHolder != null)
                        {
                            responseHolder.SenderId = id;
                            responseHolder.SentAt = DateTime.Now;

                            if (eventHolder is EventHolder<CompoundRequest> compoundRequest)
                            {
                                foreach (var subEventHolder in compoundRequest.Payload.Events)
                                {
                                    var subResponseHolder = subEventHolder.GetResponseHolder();
                                    if (subResponseHolder != null)
                                    {
                                        subResponseHolder.SenderId = id;
                                        subResponseHolder.SentAt = DateTime.Now;
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Interceptor.AfterExecute(eventHolder);
                }
            }

            return result;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();

            executionTask.Wait();

            return Task.CompletedTask;
        }
    }
}
