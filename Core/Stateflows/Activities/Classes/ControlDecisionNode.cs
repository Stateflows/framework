using Stateflows.Common.Extensions;

namespace Stateflows.Activities
{
    public sealed class ControlDecisionNode : ActivityNode
    {
        public static string Name => typeof(DecisionNode<ControlToken>).GetReadableName();
    }
}
