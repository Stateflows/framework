namespace Stateflows.Common;

/// <summary>Requests nonblocking admission of an activity token input to a live stream.</summary>
/// <remarks>
/// Stream admission is supported only by runtimes that provide a live activity execution session.
/// It is valid only on <see cref="TokensInputEvent"/> payloads.
/// </remarks>
public sealed class Stream : EventHeader;
