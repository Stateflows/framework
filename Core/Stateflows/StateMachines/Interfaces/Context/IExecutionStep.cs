namespace Stateflows.StateMachines
{
    public interface IExecutionStep
    {
        string SourceName { get; }

        string TargetName { get; }

        object TransitionTrigger { get; }
    }
}
