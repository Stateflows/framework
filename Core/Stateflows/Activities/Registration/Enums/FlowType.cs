namespace Stateflows.Activities
{
    /// <summary>Identifies the kind of activity graph flow.</summary>
    public enum FlowType
    {
        /// <summary>Control-only flow that does not carry object tokens.</summary>
        ControlFlow,
        /// <summary>Object flow that transfers tokens without changing their type.</summary>
        ObjectFlow,
        /// <summary>Object flow that transforms tokens to another type during transfer.</summary>
        ObjectTransformationFlow,
    }
}
