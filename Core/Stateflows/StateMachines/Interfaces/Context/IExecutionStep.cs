namespace Stateflows.StateMachines
{
    public interface IExecutionStep
    {
        string SourceStateName { get; }

        string TargetStateName { get; }

        object TransitionTrigger { get; }
    }
}
