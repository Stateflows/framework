namespace Stateflows.Common;

/// <summary>Requests that an event join the sender's active transaction.</summary>
/// <remarks>Supported by the Orleans runtime. Requires an ambient transaction and cannot be used for stream admission or staged sends.</remarks>
public sealed class JoinTransaction : EventHeader;
