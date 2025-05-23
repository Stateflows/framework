using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Subscription;

namespace Stateflows.Common.Classes
{
    internal sealed class StandaloneBehavior : BaseBehavior
    {
        private readonly StateflowsEngine engine;
        public StandaloneBehavior(StateflowsEngine engine, IServiceProvider serviceProvider, BehaviorId id)
            : base(serviceProvider, id, new NotificationsHub(serviceProvider.GetRequiredService<IStateflowsCache>()))
        {
            this.engine = engine;
        }

        protected override async Task<ExecutionToken> ProcessEventAsync(BehaviorId id, EventHolder eventHolder)
        {
            var executionToken = new ExecutionToken(Id, eventHolder, ServiceProvider);
            await engine.HandleEventAsync(executionToken);

            return executionToken;
        }
    }
}
