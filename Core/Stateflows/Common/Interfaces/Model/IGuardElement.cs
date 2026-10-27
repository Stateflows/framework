using System.Threading.Tasks;
using Stateflows.Activities;
using Stateflows.StateMachines;

namespace Stateflows.Common;

/// <summary>A reusable event-independent guard that can also accept or reject behavior initialization.</summary>
/// <remarks>Both initialization contracts delegate to the guard's evaluation result.</remarks>
public interface IGuardElement : IStateMachineGuard, IActivityGuard, IInitializer<object>, IDefaultInitializer
{
    /// <inheritdoc />
    Task<bool> IInitializer<object>.OnInitializeAsync(object initializationEvent)
        => GuardAsync();

    /// <inheritdoc />
    Task<bool> IDefaultInitializer.OnInitializeAsync()
        => GuardAsync();
}

/// <summary>A reusable typed guard that can also accept or reject initialization using the same event payload.</summary>
/// <typeparam name="TEvent">The event payload evaluated by the guard.</typeparam>
public interface IGuardElement<in TEvent> : IStateMachineGuard<TEvent>, IActivityGuard<TEvent>, IInitializer<TEvent>
{
    /// <inheritdoc />
    Task<bool> IInitializer<TEvent>.OnInitializeAsync(TEvent initializationEvent)
        => GuardAsync(initializationEvent);
}