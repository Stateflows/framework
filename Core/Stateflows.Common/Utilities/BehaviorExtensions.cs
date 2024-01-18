using System.Threading.Tasks;

namespace Stateflows.Common
{
    public static class BehaviorExtensions
    {
        public static Task<RequestResult<ResetResponse>> ResetAsync(this IBehavior behavior)
            => behavior.RequestAsync(new ResetRequest());

        public static Task<RequestResult<BehaviorStatusResponse>> GetStatusAsync(this IBehavior behavior)
            => behavior.RequestAsync(new BehaviorStatusRequest());
    }
}