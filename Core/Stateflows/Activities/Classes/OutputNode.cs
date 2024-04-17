using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public sealed class OutputNode : ActivityNode
    {
        public static string Name => typeof(OutputNode).GetReadableName();
    }
}
