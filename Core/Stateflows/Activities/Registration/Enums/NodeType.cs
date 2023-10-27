namespace Stateflows.Activities
{
    public enum NodeType
    {
        Activity,
        Initial,
        Final,
        Input,
        Output,
        Action,
        Decision,
        Fork,
        Join,
        Merge,
        SignalAction,
        EventAction,
        TimeEventAction,
        StructuredActivity,
        ParallelActivity,
        IterativeActivity,
        ExceptionHandler
    }
}
