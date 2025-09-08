using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Classes;
using Stateflows.Common.Engine.Interfaces;

namespace Stateflows.Common
{
    internal class StateflowsService : IHostedService, IStateflowsTelemetry
    {
        public StateflowsService(StateflowsEngine stateflowsEngine)
        {
            StateflowsEngine = stateflowsEngine;
        }

        private readonly StateflowsEngine StateflowsEngine;
        
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        
        private Channel<ExecutionToken> EventChannel { get; } = Channel.CreateUnbounded<ExecutionToken>();
        
        private Task executionTask;
        
        private int behaviorExecutionCounter = 0;

        public ExecutionToken EnqueueEvent(BehaviorId id, EventHolder eventHolder, IServiceProvider serviceProvider)
        {
            var token = new ExecutionToken(id, eventHolder, serviceProvider);

            _ = EventChannel.Writer.WriteAsync(token);
            
            return token;
        }
        
        [DebuggerHidden]
        public Task StartAsync(CancellationToken cancellationToken)
        {
            executionTask = ExecutionTaskAsync(CancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        [DebuggerHidden]
        private async Task ExecutionTaskAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var token = await EventChannel.Reader.ReadAsync(CancellationTokenSource.Token);

                _ = Task.Run(
                    async () =>
                    {
                        lock (this)
                        {
                            behaviorExecutionCounter++;
                        }
                        
                        await StateflowsEngine.HandleEventAsync(token);
                        
                        lock (this)
                        {
                            behaviorExecutionCounter--;
                        }
                    },
                    cancellationToken
                );
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                CancellationTokenSource.Cancel();

                executionTask.Wait();
            }
            catch (Exception)
            {
            }

            return Task.CompletedTask;
        }

        public int EventQueueLength => EventChannel.Reader.Count;
        public int BehaviorExecutionsCount
        {
            get
            {
                lock (this)
                {
                    return behaviorExecutionCounter;
                }
            }
        }
    }
}