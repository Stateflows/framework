namespace Stateflows.Common.Classes
{
    /// <summary>Contains process-wide Stateflows naming settings.</summary>
    public static class StateflowsSettings
    {
        /// <summary>Gets or sets the element categories whose names use fully qualified CLR names.</summary>
        public static TypedElements FullNames { get; set; } = TypedElements.All;
    }
}