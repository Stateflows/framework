namespace Stateflows.StateMachines.Context.Classes
{
    internal class ExecutionStep : IExecutionStep
    {
        public ExecutionStep(string sourceStateName, string targetStateName, object transitionTrigger)
        {
            SourceStateName = sourceStateName;
            TargetStateName = targetStateName;
            TransitionTrigger = transitionTrigger;
        }

        public string SourceStateName { get; }

        public string TargetStateName { get; }

        public object TransitionTrigger { get; }
    }
}
