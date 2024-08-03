using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public interface IActivityNode
    { }

    public static class ActivityNode<TActivityNode>
        where TActivityNode : class, IActivityNode
    {
        public static string Name => typeof(TActivityNode).GetReadableName();
    }
}
