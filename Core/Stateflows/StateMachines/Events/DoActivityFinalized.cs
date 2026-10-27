using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>Signals completion of the activity embedded as the active state's do behavior.</summary>
/// <remarks>This system notification is not forwarded to further embedded behaviors.</remarks>
[NoForwarding]
public sealed class DoActivityFinalized : Finalized;