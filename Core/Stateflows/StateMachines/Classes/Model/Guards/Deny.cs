using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public sealed class Deny : ITransitionGuard
    {
        public Task<bool> GuardAsync()
            => Task.FromResult(false);
    }
}