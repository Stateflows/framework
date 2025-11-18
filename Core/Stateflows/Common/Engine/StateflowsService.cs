using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Classes;
using Stateflows.Common.Engine.Interfaces;
using Stateflows.Common.Registration.Builders;

namespace Stateflows.Common
{
    internal class StateflowsService : IHostedService, IStateflowsTelemetry
    {
        public StateflowsService(StateflowsEngine stateflowsEngine)
        {
            StateflowsEngine = stateflowsEngine;
            var maxConcurrency = StateflowsBuilder.MaxConcurrentBehaviorExecutions > 0
                ? StateflowsBuilder.MaxConcurrentBehaviorExecutions
                : Environment.ProcessorCount;
            ConcurrencySemaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }

        private readonly StateflowsEngine StateflowsEngine;
        
        private readonly CancellationTokenSource CancellationTokenSource = new();
        
        private Channel<ExecutionToken> EventChannel { get; } = Channel.CreateUnbounded<ExecutionToken>();

        private readonly SemaphoreSlim ConcurrencySemaphore;

        private int behaviorExecutionCounter = 0;

        public async ValueTask<ExecutionToken> EnqueueEventAsync(BehaviorId id, EventHolder eventHolder, IServiceProvider serviceProvider)
        {
            var token = new ExecutionToken(id, eventHolder, serviceProvider);

            await EventChannel.Writer.WriteAsync(token);
            
            return token;
        }
        
        [DebuggerHidden]
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = ExecutionTaskAsync(CancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        [DebuggerHidden]
        private async Task ExecutionTaskAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var token = await EventChannel.Reader.ReadAsync(cancellationToken);

                await ConcurrencySemaphore.WaitAsync(cancellationToken);

                _ = Task.Run(
                    async () =>
                    {
                        try
                        {
                            Interlocked.Increment(ref behaviorExecutionCounter);
                            await StateflowsEngine.HandleEventAsync(token);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref behaviorExecutionCounter);
                            ConcurrencySemaphore.Release();
                        }
                    },
                    cancellationToken
                );
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        public int EventQueueLength => EventChannel.Reader.Count;
        public int BehaviorExecutionsCount => System.Threading.Volatile.Read(ref behaviorExecutionCounter);
    }
}