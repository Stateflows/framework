using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;
using Stateflows.System;
using Stateflows.System.Events;

namespace Stateflows.Common.System.Classes
{
    internal class SystemWrapper : ISystemBehavior
    {
        public BehaviorId Id => Behavior.Id;

        private IBehaviorLocator Locator { get; }

        private IBehavior behavior = null;
        private IBehavior Behavior
        {
            get
            {
                if (behavior == null && !Locator.TryLocateBehavior(SystemBehavior.Id, out behavior))
                {
                    throw new StateflowsException("System behavior could not be found");
                }

                return behavior;
            }
        }

        public SystemWrapper(IBehaviorLocator locator)
        {
            Locator = locator;
        }

        public Task<RequestResult<AvailableBehaviorClassesResponse>> GetAvailableBehaviorClassesAsync()
            => Behavior.RequestAsync(new AvailableBehaviorClassesRequest());

        public Task<RequestResult<BehaviorInstancesResponse>> GetBehaviorInstancesAsync()
            => Behavior.RequestAsync(new BehaviorInstancesRequest());

        public Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => Behavior.SendAsync(@event);

        public Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
            => Behavior.RequestAsync(request);
    }
}
