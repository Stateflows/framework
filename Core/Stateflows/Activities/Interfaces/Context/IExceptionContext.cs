namespace Stateflows.Activities
{
    public interface IExceptionContext
    {
        INodeContext ProtectedNode { get; }

        INodeContext NodeOfOrigin { get; }
    }
}
