using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public abstract class ActivityNode
    {
        public ITypedActionContext Context { get; internal set; }
    }

    public static class ActivityNodeInfo<TNode>
        where TNode : ActivityNode
    {
        public static string Name => typeof(TNode).GetReadableName();
    }
}
