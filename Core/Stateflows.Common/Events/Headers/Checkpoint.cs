namespace Stateflows.Common;

/// <summary>Requests a durable checkpoint of an activity's live streaming state.</summary>
/// <remarks>This header is supported only by runtimes with activity streaming support.</remarks>
public sealed class Checkpoint : EventHeader;
