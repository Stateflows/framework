using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Classes;

namespace Stateflows.Common
{
    internal class StateflowsService : IHostedService
    {
        public StateflowsService(StateflowsEngine stateflowsEngine)
        {
            StateflowsEngine = stateflowsEngine;
        }

        private readonly StateflowsEngine StateflowsEngine;
        
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        
        private Channel<ExecutionToken> EventChannel { get; } = Channel.CreateUnbounded<ExecutionToken>();
        
        private Task executionTask;

        public ExecutionToken EnqueueEvent(BehaviorId id, EventHolder eventHolder, IServiceProvider serviceProvider)
        {
            var token = new ExecutionToken(id, eventHolder, serviceProvider);

            _ = EventChannel.Writer.WriteAsync(token);
            
            return token;
        }
        
        [DebuggerHidden]
        public Task StartAsync(CancellationToken cancellationToken)
        {
            executionTask = ExecutionTaskAsync(cancellationToken);

            return Task.CompletedTask;
        }

        [DebuggerHidden]
        private async Task ExecutionTaskAsync(CancellationToken cancellationToken)
        {
            while (!CancellationTokenSource.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                var token = await EventChannel.Reader.ReadAsync(cancellationToken);

                _ = StateflowsEngine.HandleEventAsync(token);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();

            executionTask.Wait(cancellationToken);

            return Task.CompletedTask;
        }
    }
}