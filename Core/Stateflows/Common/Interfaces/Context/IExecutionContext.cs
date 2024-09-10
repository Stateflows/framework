namespace Stateflows.Common
{
    public interface IExecutionContext
    {
        EventHolder ExecutionTrigger { get; }
    }
}
