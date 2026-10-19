using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Classes;

namespace Stateflows.Common
{
    internal class StateflowsService(StateflowsEngine stateflowsEngine, StateflowsResources resources) :
        IHostedService
    {
        private readonly CancellationTokenSource CancellationTokenSource = new();

        public async ValueTask<ExecutionToken> EnqueueEventAsync(BehaviorId id, EventHolder eventHolder, IServiceProvider serviceProvider)
        {
            var token = new ExecutionToken(id, eventHolder, serviceProvider);

            var resource = resources.ResourcesByName[resources.ResourcesByBehaviorClass[id.BehaviorClass].Name];
            
            await resource.WriteAsync(token);
            
            return token;
        }
        
        [DebuggerHidden]
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = ExecutionTaskAsync(CancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        [DebuggerHidden]
        private Task ExecutionTaskAsync(CancellationToken cancellationToken)
        {
            foreach (var resourceName in resources.ResourcesByName.Values)
            {
                _ = Task.Run(
                    async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var token = await resourceName.ReadAsync(cancellationToken);

                            _ = Task.Run(
                                async () =>
                                {
                                    try
                                    {
                                        await stateflowsEngine.HandleEventAsync(token);
                                    }
                                    finally
                                    {
                                        await resourceName.ReleaseAsync(cancellationToken);
                                    }
                                },
                                cancellationToken
                            );
                        }
                    },
                    cancellationToken
                );
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }
    }
}