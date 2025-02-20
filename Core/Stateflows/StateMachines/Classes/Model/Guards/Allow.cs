using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public sealed class Allow : ITransitionGuard
    {
        public Task<bool> GuardAsync()
            => Task.FromResult(true);
    }
}