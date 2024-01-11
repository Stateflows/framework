using System.Threading.Tasks;
using Stateflows.System;

namespace Stateflows.Common.System.Classes
{
    internal class SystemWrapper : ISystem
    {
        private readonly IBehavior Behavior;

        public SystemWrapper(IBehavior behavior)
        {
            Behavior = behavior;
        }

        public Task<RequestResult<AvailableBehaviorClassesResponse>> GetAvailableBehaviorClassesAsync()
            => Behavior.RequestAsync(new AvailableBehaviorClassesRequest());

        public Task<RequestResult<BehaviorInstancesResponse>> GetBehaviorInstancesAsync()
            => Behavior.RequestAsync(new BehaviorInstancesRequest());
    }
}
