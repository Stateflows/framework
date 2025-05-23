using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Subscription;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Classes
{
    internal sealed class Behavior : BaseBehavior
    {
        private readonly StateflowsService service;
        public Behavior(StateflowsService service, IServiceProvider serviceProvider, BehaviorId id)
            : base(serviceProvider, id, serviceProvider.GetRequiredService<NotificationsHub>())
        {
            this.service = service;
        }

        protected override async Task<ExecutionToken> ProcessEventAsync(BehaviorId id, EventHolder eventHolder)
        {
            var executionToken = service.EnqueueEvent(Id, eventHolder, serviceProvider);
            await executionToken.Handled.WaitOneAsync().ConfigureAwait(false);

            return executionToken;
        }
    }
}