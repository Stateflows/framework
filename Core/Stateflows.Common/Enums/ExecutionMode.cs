namespace Stateflows.Common;

/// <summary>Identifies the ownership of the current behavior invocation.</summary>
public enum ExecutionMode
{
    /// <summary>Invocation owned by a regular event transaction.</summary>
    Transactional,
    /// <summary>Token-enabled invocation owned by a live Activity stream, including mixed input.</summary>
    Streamed
}
