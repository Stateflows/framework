namespace Stateflows.Activities
{
    /// <summary>Identifies the activity nodes involved in an exception-handling operation.</summary>
    public interface IExceptionContext
    {
        /// <summary>The node whose exception-handler scope is protecting execution.</summary>
        INodeContext ProtectedNode { get; }

        /// <summary>The node at which the exception originated.</summary>
        INodeContext NodeOfOrigin { get; }
    }
}
