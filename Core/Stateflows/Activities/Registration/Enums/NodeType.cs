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
        SendEventAction,
        AcceptEventAction,
        StructuredActivity,
        ParallelActivity,
        IterativeActivity,
        ExceptionHandler,
        DataStore,
    }
}
