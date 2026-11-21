namespace Stateflows.Common;

/// <summary>Identifies whether the current behavior invocation is transactional or streamed.</summary>
public enum ExecutionMode
{
    /// <summary>Invocation owned by a transactional event execution.</summary>
    Transactional,
    /// <summary>Token-enabled invocation owned by a live Activity stream, including mixed input.</summary>
    Streamed
}
