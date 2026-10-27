namespace Stateflows.Common
{
    /// <summary>Specifies which processing exceptions are propagated to the caller.</summary>
    public enum ExceptionPropagationMode
    {
        /// <summary>Do not propagate processing exceptions.</summary>
        NoPropagation,
        /// <summary>Propagate only exceptions that were not handled by the behavior.</summary>
        PropagateUnhandled,
        /// <summary>Propagate every processing exception.</summary>
        PropagateAll
    }
}
