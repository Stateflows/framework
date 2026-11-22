namespace Stateflows.Activities;

/// <summary>Identifies the activity element that declares a set of payload token names.</summary>
public enum TokenNamesDeclarationKind
{
    InputPin,
    OutputPin,
    FlowSource,
    FlowTarget,
    TransformationProduced,
    Decision,
    IterativeActivity,
    ParallelActivity
}
