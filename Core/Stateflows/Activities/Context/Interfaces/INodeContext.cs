using System.Collections.Generic;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface INodeContext
    {
        string NodeName { get; }

        IDictionary<string, object> Values { get; }

        bool TryGetParentNode(out INodeContext parentNodeContext);
    }
}
