namespace Stateflows.Common.Interfaces;

/// <summary>Marks an element as a consumer of a specified event payload type.</summary>
/// <typeparam name="TEvent">The consumed event payload type.</typeparam>
public interface IEventConsumer<TEvent>;