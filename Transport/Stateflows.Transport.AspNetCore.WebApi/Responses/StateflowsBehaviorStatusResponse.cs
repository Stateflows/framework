using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Transport.AspNetCore.WebApi.Responses
{
    internal class StateflowsBehaviorStatusResponse
    {
        public StateflowsBehaviorStatusResponse(RequestResult<BehaviorStatusResponse> result)
        {
            BehaviorStatus = result.Response?.BehaviorStatus ?? BehaviorStatus.NotInitialized;
        }

        public BehaviorStatus BehaviorStatus { get; set; }
    }
}
