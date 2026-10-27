namespace Stateflows.Common.Interfaces;

/// <summary>Marks an element as a producer of a specified token payload type.</summary>
/// <typeparam name="TEvent">The produced token payload type.</typeparam>
public interface ITokenProducer<TEvent>;