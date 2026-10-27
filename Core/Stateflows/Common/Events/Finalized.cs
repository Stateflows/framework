namespace Stateflows.Common;

/// <summary>Signals that a behavior has finalized.</summary>
[NoForwarding]
public class Finalized : SystemEvent
{
    /// <summary>Gets or sets whether finalization was forced rather than reached through normal completion.</summary>
    public bool ForcefulFinalization { get; set; } = false;
}