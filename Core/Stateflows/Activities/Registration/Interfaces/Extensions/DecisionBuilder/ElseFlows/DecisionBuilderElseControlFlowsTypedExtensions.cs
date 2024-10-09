using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class DecisionBuilderElseControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IDecisionBuilder AddElseFlow<TTargetNode>(this IDecisionBuilder builder, ElseControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddElseFlow(ActivityNode<TTargetNode>.Name, b => buildAction?.Invoke(b));
    }
}
