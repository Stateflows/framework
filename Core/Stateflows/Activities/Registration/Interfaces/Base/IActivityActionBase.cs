using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration.Interfaces.Base;

/// <summary>Provides methods for adding executable action nodes to an activity.</summary>
/// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
/// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
public interface IActivityActionBase<out TReturn>
{
    #region AddAction
    /// <summary>Adds an executable action node to the Activity graph.</summary>
    /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
    /// <param name="actionAsync">Asynchronous callback invoked when the node activates; it receives the node's action
    /// context.</param>
    /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
    /// </param>
    /// <returns>The current builder.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    TReturn AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync, ActionBuildAction buildAction = null);
        
    /// <summary>Adds an executable action node to the Activity graph.</summary>
    /// <typeparam name="TAction">Typed node implementation implementing <see cref="IActionNode" />.</typeparam>
    /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
    /// </param>
    /// <returns>The current builder.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    [DebuggerHidden]
    public TReturn AddAction<TAction>(TypedActionBuildAction<TAction> buildAction = null)
        where TAction : class, IActionNode
        => AddAction<TAction>(ActivityNode<TAction>.Name, buildAction);

    /// <summary>Adds an executable action node to the Activity graph.</summary>
    /// <typeparam name="TAction">Typed node implementation implementing <see cref="IActionNode" />.</typeparam>
    /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
    /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
    /// </param>
    /// <returns>The current builder.</returns>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-action-node" />
    [DebuggerHidden]
    public TReturn AddAction<TAction>(string actionNodeName, TypedActionBuildAction<TAction> buildAction = null)
        where TAction : class, IActionNode
    {
        var result = AddAction(
            actionNodeName,
            async c =>
            {
                var context = (BaseContext)c;
                var action = await context.NodeScope.GetActionAsync<TAction>(c);
                
                context.NodeScope.Node.ConfigurationAction?.Invoke(action);

                InputTokens.TokensHolder.Value = ((ActionContext)c).InputTokens;
                OutputTokens.TokensHolder.Value = ((ActionContext)c).OutputTokens;

                ActivityNodeContextAccessor.Context.Value = c;
                await action.ExecuteAsync(c.CancellationToken);
                ActivityNodeContextAccessor.Context.Value = null;
            },
            b =>
            {
                var nodeBuilder = (NodeBuilder)b;
                nodeBuilder.Node.ScanForDeclaredTypes(typeof(TAction));
                buildAction?.Invoke(new ActionNodeBuilder<TAction>(nodeBuilder.Node, nodeBuilder.ActivityBuilder));
            }
        );

        var graph = ((IGraphBuilder)this).Graph;
        graph.VisitingTasks.Add(visitor => visitor.NodeTypeAddedAsync<TAction>(graph.Name, graph.Version, actionNodeName));

        return result;
    }
    #endregion
}