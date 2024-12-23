namespace Stateflows.StateMachines.Context.Classes
{
    internal class ExecutionStep : IExecutionStep
    {
        public ExecutionStep(string sourceStateName, string targetStateName, object transitionTrigger)
        {
            SourceName = sourceStateName;
            TargetName = targetStateName;
            TransitionTrigger = transitionTrigger;
        }

        public string SourceName { get; }

        public string TargetName { get; }

        public object TransitionTrigger { get; }
    }
}
