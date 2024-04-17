using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public sealed class ForkNode : ActivityNode
    {
        public static string Name => typeof(ForkNode).GetReadableName();
    }
}
