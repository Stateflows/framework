namespace Stateflows.Common.Interfaces;

/// <summary>Marks an element as a producer of a specified event payload type.</summary>
/// <typeparam name="TEvent">The produced event payload type.</typeparam>
public interface IEventProducer<TEvent>;