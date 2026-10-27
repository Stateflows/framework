using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>A guard that always allows the guarded operation or initialization.</summary>
    public sealed class Allow : IGuardElement
    {
        /// <summary>Allows the operation unconditionally.</summary>
        /// <returns>A completed task containing <see langword="true"/>.</returns>
        public Task<bool> GuardAsync()
            => Task.FromResult(true);
    }
    
    /// <summary>A typed guard that always allows the operation, regardless of the event payload.</summary>
    /// <typeparam name="TEvent">The event payload type accepted by the guard.</typeparam>
    public sealed class Allow<TEvent> : IGuardElement<TEvent>
    {
        /// <summary>Allows the operation without inspecting the event.</summary>
        /// <param name="event">The event payload, which is not inspected.</param>
        /// <returns>A completed task containing <see langword="true"/>.</returns>
        public Task<bool> GuardAsync(TEvent @event)
            => Task.FromResult(true);
    }
}