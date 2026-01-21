using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        
        private readonly CancellationTokenSource CancellationTokenSource = new();

        public async ValueTask<ExecutionToken> EnqueueEventAsync(BehaviorId id, EventHolder eventHolder, IServiceProvider serviceProvider)
        {
            var token = new ExecutionToken(id, eventHolder, serviceProvider);

            var resource = StateflowsEngine.StateflowsBuilder.ResourcesByBehaviorClass[id.BehaviorClass];
            
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
            foreach (var resourceName in StateflowsEngine.StateflowsBuilder.ResourceNames.Values)
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
                                        await StateflowsEngine.HandleEventAsync(token);
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

        public IEnumerable<IStateflowsResource> Resources => StateflowsEngine.StateflowsBuilder.ResourceNames.Values;

        private Dictionary<BehaviorClass, IStateflowsResource> resourcesByBehaviorClass;
        public IReadOnlyDictionary<BehaviorClass, IStateflowsResource> ResourcesByBehaviorClass
        {
            get
            {
                if (resourcesByBehaviorClass == null)
                {
                    resourcesByBehaviorClass = new Dictionary<BehaviorClass, IStateflowsResource>();
                    foreach (var pair in  StateflowsEngine.StateflowsBuilder.ResourcesByBehaviorClass)
                    {
                        resourcesByBehaviorClass[pair.Key] = pair.Value;
                    }
                }

                return resourcesByBehaviorClass;
            }
        } 
    }
}