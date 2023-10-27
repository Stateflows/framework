using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class ActivityNode
    {
        public IActionContext Context { get; internal set; }
    }

    public static class ActivityNodeInfo<TNode>
        where TNode : ActivityNode
    {
        public static string Name => typeof(TNode).FullName;
    }
}
