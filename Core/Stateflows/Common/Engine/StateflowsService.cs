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
            
            var counter = EventQueue.Enqueue(token);
            token.Counter = counter;
            
            return token;
        }
        
        [DebuggerHidden]
        public Task StartAsync(CancellationToken cancellationToken)
        {
            executionTask = Task.Run(() =>
            {
                while (!CancellationTokenSource.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    if (!EventQueue.WaitAsync(CancellationTokenSource.Token).GetAwaiter().GetResult())
                    {
                        continue;
                    }

                    var token = EventQueue.Dequeue();

                    if (token != null)
                    {
                        _ = Task.Run(() => StateflowsEngine.HandleEventAsync(token), cancellationToken);
                    }
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();

            executionTask.Wait(cancellationToken);

            return Task.CompletedTask;
        }
    }
}