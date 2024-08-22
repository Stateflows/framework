namespace Stateflows.Common
{
    public interface IExecutionContext
    {
        Event ExecutionTrigger { get; }
    }
}
