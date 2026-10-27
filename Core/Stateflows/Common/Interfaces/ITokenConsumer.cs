namespace Stateflows.Common.Interfaces;

/// <summary>Marks an element as a consumer of a specified token payload type.</summary>
/// <typeparam name="TEvent">The consumed token payload type.</typeparam>
public interface ITokenConsumer<TEvent>;