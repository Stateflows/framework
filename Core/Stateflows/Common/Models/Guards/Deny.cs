using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>A guard that always rejects the guarded operation or initialization.</summary>
    public sealed class Deny : IGuardElement
    {
        /// <summary>Rejects the operation unconditionally.</summary>
        /// <returns>A completed task containing <see langword="false"/>.</returns>
        public Task<bool> GuardAsync()
            => Task.FromResult(false);
    }
    
    /// <summary>A typed guard that always rejects the operation, regardless of the event payload.</summary>
    /// <typeparam name="TEvent">The event payload type accepted by the guard.</typeparam>
    public sealed class Deny<TEvent> : IGuardElement<TEvent>
    {
        /// <summary>Rejects the operation without inspecting the event.</summary>
        /// <param name="event">The event payload, which is not inspected.</param>
        /// <returns>A completed task containing <see langword="false"/>.</returns>
        public Task<bool> GuardAsync(TEvent @event)
            => Task.FromResult(false);
    }
}