using System.Threading.Tasks;

namespace Stateflows.Common
{
    public sealed class Deny : IGuardElement
    {
        public Task<bool> GuardAsync()
            => Task.FromResult(false);
    }
    
    public sealed class Deny<TEvent> : IGuardElement<TEvent>
    {
        public Task<bool> GuardAsync(TEvent @event)
            => Task.FromResult(false);
    }
}