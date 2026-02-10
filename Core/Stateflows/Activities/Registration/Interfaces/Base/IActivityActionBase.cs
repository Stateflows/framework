using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration.Interfaces.Base;

public interface IActivityActionBase<out TReturn>
{
    #region AddAction
    TReturn AddAction(string actionNodeName, Func<IActionContext, Task> actionAsync, ActionBuildAction buildAction = null);
        
    [DebuggerHidden]
    public TReturn AddAction<TAction>(TypedActionBuildAction<TAction> buildAction = null)
        where TAction : class, IActionNode
        => AddAction<TAction>(ActivityNode<TAction>.Name, buildAction);

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