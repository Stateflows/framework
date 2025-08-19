using System;
using System.Diagnostics;
using System.Threading;
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
        
        private EventQueue<ExecutionToken> EventQueue { get; } = new EventQueue<ExecutionToken>(true);
        
        private Task executionTask;

        public ExecutionToken EnqueueEvent(BehaviorId id, EventHolder eventHolder, IServiceProvider serviceProvider)
        {
            var token = new ExecutionToken(id, eventHolder, serviceProvider);

            EventQueue.Enqueue(token);
            
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
                if (!await EventQueue.WaitAsync(CancellationTokenSource.Token))
                {
                    continue;
                }

                var token = EventQueue.Dequeue();

                if (token != null)
                {
                    _ = StateflowsEngine.HandleEventAsync(token);
                }
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