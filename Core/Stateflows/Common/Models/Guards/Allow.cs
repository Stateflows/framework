using System.Threading.Tasks;

namespace Stateflows.Common
{
    public sealed class Allow : IGuardElement
    {
        public Task<bool> GuardAsync()
            => Task.FromResult(true);
    }
    
    public sealed class Allow<TEvent> : IGuardElement<TEvent>
    {
        public Task<bool> GuardAsync(TEvent @event)
            => Task.FromResult(true);
    }
}