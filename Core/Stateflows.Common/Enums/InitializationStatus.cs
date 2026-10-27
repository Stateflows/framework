namespace Stateflows.Common
{
    /// <summary>Describes the result of attempting to initialize a behavior.</summary>
    public enum InitializationStatus
    {
        /// <summary>The behavior was initialized by an explicit initialization request.</summary>
        InitializedExplicitly,
        /// <summary>The behavior was initialized implicitly while processing another event.</summary>
        InitializedImplicitly,
        /// <summary>The behavior remains uninitialized.</summary>
        NotInitialized,
        /// <summary>No initializer could handle the requested initialization event.</summary>
        NoSuitableInitializer
    }
}
