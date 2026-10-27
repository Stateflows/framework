using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base;

/// <summary>Replaces action nodes inherited from a base activity definition.</summary>
/// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
public interface IActivityActionOverrides<out TReturn>
{
    #region UseAction
    /// <summary>Selects and replaces the matching inherited node.</summary>
    /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
    /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
    /// builder is scoped to that element.</param>
    /// <returns>The current builder.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    TReturn UseAction(string actionNodeName, OverridenActionBuildAction buildAction);
        
    /// <summary>Selects and replaces the matching inherited node.</summary>
    /// <typeparam name="TAction">Typed node implementation implementing <see cref="IActionNode" />.</typeparam>
    /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
    /// builder is scoped to that element.</param>
    /// <returns>The current builder.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
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