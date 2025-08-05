using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Classes;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Common
{
    internal class StateflowsService : IHostedService//, IStateflowsInitializer
    {
        public StateflowsService(StateflowsEngine stateflowsEngine, /*IStateflowsBehaviorsBuilder behaviorsBuilder,*/ IServiceProvider serviceProvider)
        {
            StateflowsEngine = stateflowsEngine;
            // BehaviorsBuilder = behaviorsBuilder;
            ServiceProvider = serviceProvider;
        }
        
        // private bool behaviorsBuilt = false;
        //
        // public void Initialize(IServiceProvider serviceProvider)
        // {
        //     lock (this)
        //     {
        //         if (behaviorsBuilt) return;
        //
        //         foreach (var register in BehaviorsBuilder.Registers)
        //         {
        //             register.Build(serviceProvider);
        //         }
        //
        //         behaviorsBuilt = true;
        //     }
        // }

        private readonly StateflowsEngine StateflowsEngine;

        // private readonly IStateflowsBehaviorsBuilder BehaviorsBuilder;

        private readonly IServiceProvider ServiceProvider;
        
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
            // Initialize(ServiceProvider);
            
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