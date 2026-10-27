using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>Signals finalization of an embedded state machine to its containing machine.</summary>
/// <remarks>This system notification is not forwarded to further embedded behaviors.</remarks>
[NoForwarding]
public sealed class SubmachineFinalized : Finalized
{ }