using Stateflows.Common.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class ActivityNode
    {
        public ITypedActionContext Context
            => (ITypedActionContext)ActivityNodeContextAccessor.Context.Value;
    }

    public static class ActivityNodeInfo<TNode>
        where TNode : ActivityNode
    {
        public static string Name => typeof(TNode).GetReadableName();
    }
}
