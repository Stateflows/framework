using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows
{
    public static class BehaviorExtensions
    {
        public static Task<RequestResult<InitializationResponse>> InitializeAsync(this IBehavior behavior, InitializationRequest initializationRequest = null)
            => behavior.RequestAsync(initializationRequest ?? new InitializationRequest());

        public static Task<RequestResult<BehaviorStatusResponse>> GetStatusAsync(this IBehavior behavior)
            => behavior.RequestAsync(new BehaviorStatusRequest());
    }
}