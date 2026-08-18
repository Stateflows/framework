using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base;

public interface IActivityActionOverrides<out TReturn>
{
    #region UseAction
    TReturn UseAction(string actionNodeName, OverridenActionBuildAction buildAction);
        
    [DebuggerHidden]
    public TReturn UseAction<TAction>(OverridenTypedActionBuildAction<TAction> buildAction)
        where TAction : class, IActionNode
        => UseAction(ActivityNode<TAction>.Name, b =>
        {
            var nodeBuilder = (NodeBuilder)b;
            buildAction?.Invoke(new ActionNodeBuilder<TAction>(nodeBuilder.Node, nodeBuilder.ActivityBuilder));
        });
    #endregion
}