using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces;

/// <summary>Defines an asynchronous guard that evaluates an event payload.</summary>
/// <typeparam name="TEvent">The event payload type received by the guard.</typeparam>
public interface IAbstractGuard<in TEvent> : IAbstractElement
{
    /// <summary>Determines whether the guarded operation is allowed for the event.</summary>
    /// <param name="event">The event payload being evaluated.</param>
    /// <returns><see langword="true"/> when the guarded operation may proceed.</returns>
    Task<bool> GuardAsync(TEvent @event);
}
    
/// <summary>Defines an asynchronous guard that does not require an event payload.</summary>
public interface IAbstractGuard : IAbstractGuard<object>
{
    /// <summary>Determines whether the guarded operation is allowed.</summary>
    /// <returns><see langword="true"/> when the guarded operation may proceed.</returns>
    Task<bool> GuardAsync();
        
    /// <inheritdoc />
    Task<bool> IAbstractGuard<object>.GuardAsync(object @event)
        => GuardAsync();
}