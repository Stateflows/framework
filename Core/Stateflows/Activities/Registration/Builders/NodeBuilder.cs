using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Registration
{
    internal class NodeBuilder(Node node, BaseActivityBuilder activityBuilder) :
        IActionBuilder,
        IOverridenActionBuilder,
        ITimeEventActionBuilder,
        IOverridenTimeEventActionBuilder,
        ISendEventActionBuilder,
        IOverridenSendEventActionBuilder,
        IPublishEventActionBuilder,
        IOverridenPublishEventActionBuilder,
        IInitialBuilder,
        IOverridenInitialBuilder,
        IInputBuilder,
        IOverridenInputBuilder,
        IMergeBuilder,
        IOverridenMergeBuilder,
        IJoinBuilder,
        IOverridenJoinBuilder,
        IForkBuilder,
        IOverridenForkBuilder,
        IDecisionBuilder,
        IOverridenDecisionBuilder,
        IDataStoreBuilder,
        IOverridenDataStoreBuilder
    {
        public Node Node { get; } = node;

        public Graph Graph => Node.Graph;

        public BaseActivityBuilder ActivityBuilder { get; } = activityBuilder;

        public IActionBuilder AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null)
        {
            var result = AddFlowInternal<ControlToken>(targetNodeName, false, b => buildAction?.Invoke(b as IControlFlowBuilder));
            
            Graph.VisitingTasks.Add(v => v.ControlFlowAddedAsync(Graph.Name, Graph.Version, Node.Name, targetNodeName, false));
            
            return result;
        }

        public IActionBuilder AddElseControlFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null)
        {
            var result = AddFlowInternal<ControlToken>(targetNodeName, true, b => buildAction?.Invoke(b as IElseControlFlowBuilder));
            
            Graph.VisitingTasks.Add(v => v.ControlFlowAddedAsync(Graph.Name, Graph.Version, Node.Name, targetNodeName, true));
            
            return result;
        }

        public IActionBuilder AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
        {
            var result = AddFlowInternal<TToken>(targetNodeName, false, buildAction);
            
            Graph.VisitingTasks.Add(v => v.FlowAddedAsync<TToken>(Graph.Name, Graph.Version, Node.Name, targetNodeName, false));
            
            return result;
        }

        public IActionBuilder AddElseFlow<TToken>(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null)
        {
            var result = AddFlowInternal<TToken>(targetNodeName, true, b => buildAction?.Invoke(b as IElseObjectFlowBuilder<TToken>));
            
            Graph.VisitingTasks.Add(v => v.FlowAddedAsync<TToken>(Graph.Name, Graph.Version, Node.Name, targetNodeName, true));
            
            return result;
        }

        public IActionBuilder AddFlowInternal<TToken>(string targetNodeName, bool isElse, ObjectFlowBuildAction<TToken> buildAction = null)
        {
            if (Node.Parent.Type != NodeType.Activity)
            {
                targetNodeName = $"{Node.Parent.Name}.{targetNodeName}";
            }

            var edge = new Edge()
            {
                TokenType = typeof(TToken),
                TargetTokenType = typeof(TToken),
                Graph = Node.Graph,
                SourceName = Node.Name,
                Source = Node,
                TargetName = targetNodeName,
                IsElse = isElse
            };

            Node.Edges.Add(edge);
            Node.Graph.AllEdgesList.Add(edge);

            buildAction?.Invoke(new FlowBuilder<TToken>(edge));

            return this;
        }

        public IActionBuilder SetOptions(NodeOptions nodeOptions)
        {
            Node.Options = nodeOptions;

            return this;
        }

        IOverridenActionBuilder INodeOptions<IOverridenActionBuilder>.UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater)
            => UpdateOptions(nodeOptionsUpdater) as IOverridenActionBuilder;

        public IActionBuilder UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater)
        {
            Node.Options = nodeOptionsUpdater(Node.Options);

            return this;
        }

        IInitialBuilder IControlFlowBase<IInitialBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IInitialBuilder;

        public IActionBuilder AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            where TException : Exception
        {
            var targetNodeName = $"{Node.Name}.{typeof(TException).FullName}.ExceptionHandler";

            AddFlow<TException>(targetNodeName);
            AddFlow<NodeReferenceToken>(targetNodeName);

            ActivityBuilder.AddNode(
                NodeType.ExceptionHandler,
                targetNodeName,
                c =>
                {
                    var contextObj = c as ActionContext;
                    var nodeOfOrigin = contextObj.InputTokens.OfType<TokenHolder<NodeReferenceToken>>().FirstOrDefault()?.Payload?.Node;
                    var context = new ExceptionHandlerContext<TException>(contextObj, Node, nodeOfOrigin, contextObj.NodeScope);

                    exceptionHandler?.Invoke(context);

                    contextObj.OutputTokens.AddRange(context.OutputTokens);

                    return Task.CompletedTask;
                },
                null,
                typeof(TException)
            );

            return this;
        }

        IActionBuilder IExceptionHandlerBase<IActionBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IActionBuilder;

        IInputBuilder IObjectFlowBase<IInputBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IInputBuilder;

        void IObjectFlowBase.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction);

        void IControlFlowBase.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction);

        IForkBuilder IObjectFlowBase<IForkBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IForkBuilder;

        IForkBuilder IControlFlowBase<IForkBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IForkBuilder;

        ITimeEventActionBuilder IObjectFlowBase<ITimeEventActionBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as ITimeEventActionBuilder;

        ITimeEventActionBuilder IControlFlowBase<ITimeEventActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as ITimeEventActionBuilder;

        ITimeEventActionBuilder IExceptionHandlerBase<ITimeEventActionBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as ITimeEventActionBuilder;

        ISendEventActionBuilder IControlFlowBase<ISendEventActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as ISendEventActionBuilder;

        IDecisionBuilder IDecisionFlowBase<IDecisionBuilder>.AddFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IDecisionBuilder;

        IDecisionBuilder IElseDecisionFlowBase<IDecisionBuilder>.AddElseFlow(string targetNodeName, ElseControlFlowBuildAction buildAction)
            => AddElseControlFlow(targetNodeName, buildAction) as IDecisionBuilder;

        IDataStoreBuilder IObjectFlowBase<IDataStoreBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IDataStoreBuilder;

        IOverridenInitialBuilder IControlFlowBase<IOverridenInitialBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenInitialBuilder;

        IOverridenInputBuilder IObjectFlowBase<IOverridenInputBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IOverridenInputBuilder;

        IOverridenForkBuilder IObjectFlowBase<IOverridenForkBuilder>.AddFlow<TToken>(string targetNodeName,
            ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IOverridenForkBuilder;

        IOverridenForkBuilder IControlFlowBase<IOverridenForkBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenForkBuilder;

        protected NodeBuilder UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
        {
            var edge = Node.Edges.FirstOrDefault(edge =>
                edge.TargetName == targetNodeName &&
                edge.TokenType == typeof(TToken) &&
                !edge.IsElse
            );
            
            if (edge?.OriginActivityName == null)
            {
                throw new ActivityOverrideException($"Flow targeting '{targetNodeName}' not found in overriden node '{Node.Name}'", Node.Graph.Class);
            }
            
            buildAction(new FlowBuilder<TToken>(edge));

            return this;
        }

        protected NodeBuilder UseElseFlow<TToken>(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction)
        {
            var edge = Node.Edges.FirstOrDefault(edge =>
                edge.TargetName == targetNodeName &&
                edge.TokenType == typeof(TToken) &&
                edge.IsElse
            );
            
            if (edge?.OriginActivityName == null)
            {
                throw new ActivityOverrideException($"Else flow targeting '{targetNodeName}' not found in overriden node '{Node.Name}'", Node.Graph.Class);
            }
            
            buildAction(new FlowBuilder<TToken>(edge));

            return this;
        }

        protected NodeBuilder UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseFlow<ControlToken>(targetNodeName, b => buildAction(b as IControlFlowBuilder));

        protected NodeBuilder UseElseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseElseFlow<ControlToken>(targetNodeName, b => buildAction(b as IControlFlowBuilder));

        IOverridenInitialBuilder IOverridenControlFlowBase<IOverridenInitialBuilder>.UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseControlFlow(targetNodeName, buildAction);

        IOverridenInputBuilder IOverridenObjectFlowBase<IOverridenInputBuilder>.UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => UseFlow(targetNodeName, buildAction);

        void IOverridenObjectFlowBase.UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => UseFlow<TToken>(targetNodeName, buildAction);

        void IOverridenControlFlowBase.UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseControlFlow(targetNodeName, buildAction);

        IOverridenForkBuilder IOverridenObjectFlowBase<IOverridenForkBuilder>.UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => UseFlow<TToken>(targetNodeName, buildAction);

        IOverridenForkBuilder IOverridenControlFlowBase<IOverridenForkBuilder>.UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseControlFlow(targetNodeName, buildAction);

        IPublishEventActionBuilder IControlFlowBase<IPublishEventActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IPublishEventActionBuilder;

        IOverridenActionBuilder IObjectFlowBase<IOverridenActionBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow(targetNodeName, buildAction) as IOverridenActionBuilder;

        IOverridenActionBuilder IControlFlowBase<IOverridenActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenActionBuilder;

        IOverridenActionBuilder IExceptionHandlerBase<IOverridenActionBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IOverridenActionBuilder;

        IOverridenTimeEventActionBuilder IObjectFlowBase<IOverridenTimeEventActionBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow(targetNodeName, buildAction) as IOverridenTimeEventActionBuilder;

        IOverridenTimeEventActionBuilder IOverridenObjectFlowBase<IOverridenTimeEventActionBuilder>.UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => UseFlow(targetNodeName, buildAction) as IOverridenTimeEventActionBuilder;

        IOverridenTimeEventActionBuilder IControlFlowBase<IOverridenTimeEventActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenTimeEventActionBuilder;

        IOverridenTimeEventActionBuilder IOverridenControlFlowBase<IOverridenTimeEventActionBuilder>.UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseControlFlow(targetNodeName, buildAction) as IOverridenTimeEventActionBuilder;

        IOverridenTimeEventActionBuilder IExceptionHandlerBase<IOverridenTimeEventActionBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler(exceptionHandler) as IOverridenTimeEventActionBuilder;

        IOverridenSendEventActionBuilder IControlFlowBase<IOverridenSendEventActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenSendEventActionBuilder;

        IOverridenSendEventActionBuilder IOverridenControlFlowBase<IOverridenSendEventActionBuilder>.UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseControlFlow(targetNodeName, buildAction) as IOverridenSendEventActionBuilder;

        IOverridenDecisionBuilder IOverridenDecisionFlowBase<IOverridenDecisionBuilder>.UseFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseControlFlow(targetNodeName, b => buildAction(b as IControlFlowBuilder)) as IOverridenDecisionBuilder;

        IOverridenDecisionBuilder IOverridenElseDecisionFlowBase<IOverridenDecisionBuilder>.UseElseFlow(string targetNodeName, ElseControlFlowBuildAction buildAction)
            => UseElseControlFlow(targetNodeName, b => buildAction(b as IElseControlFlowBuilder)) as IOverridenDecisionBuilder;

        IOverridenDataStoreBuilder IObjectFlowBase<IOverridenDataStoreBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow(targetNodeName, buildAction) as IOverridenDataStoreBuilder;

        IOverridenDataStoreBuilder IOverridenObjectFlowBase<IOverridenDataStoreBuilder>.UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => UseFlow(targetNodeName, buildAction) as IOverridenDataStoreBuilder;

        IOverridenPublishEventActionBuilder IControlFlowBase<IOverridenPublishEventActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenPublishEventActionBuilder;

        IOverridenPublishEventActionBuilder IOverridenControlFlowBase<IOverridenPublishEventActionBuilder>.UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseControlFlow(targetNodeName, buildAction) as IOverridenPublishEventActionBuilder;

        IOverridenActionBuilder INodeOptions<IOverridenActionBuilder>.SetOptions(NodeOptions nodeOptions)
            => SetOptions(nodeOptions) as IOverridenActionBuilder;
    }

    internal class ActionNodeBuilder<TNode>(Node node, BaseActivityBuilder activityBuilder) :
        NodeBuilder(node, activityBuilder),
        ITypedActionBuilder<TNode>,
        IOverridenTypedActionBuilder<TNode>
        where TNode : class, IActionNode
    {
        public ITypedActionBuilder<TNode> Configure(Action<TNode> action)
        {
            Node.ConfigurationAction = o => action((TNode)o);
            
            return this;
        }
        
        ITypedActionBuilder<TNode> IObjectFlowBase<ITypedActionBuilder<TNode>>.AddFlow<TToken>(string targetNodeName,
            ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as ITypedActionBuilder<TNode>;

        ITypedActionBuilder<TNode> IControlFlowBase<ITypedActionBuilder<TNode>>.AddControlFlow(string targetNodeName,
            ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as ITypedActionBuilder<TNode>;

        ITypedActionBuilder<TNode> IExceptionHandlerBase<ITypedActionBuilder<TNode>>.AddExceptionHandler<TException>(
            ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as ITypedActionBuilder<TNode>;

        IOverridenTypedActionBuilder<TNode> IObjectFlowBase<IOverridenTypedActionBuilder<TNode>>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow(targetNodeName, buildAction) as IOverridenTypedActionBuilder<TNode>;

        IOverridenTypedActionBuilder<TNode> IControlFlowBase<IOverridenTypedActionBuilder<TNode>>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenTypedActionBuilder<TNode>;

        IOverridenTypedActionBuilder<TNode> IExceptionHandlerBase<IOverridenTypedActionBuilder<TNode>>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IOverridenTypedActionBuilder<TNode>;

        IOverridenTypedActionBuilder<TNode> IElementBuilderBase<TNode, IOverridenTypedActionBuilder<TNode>>.Configure(Action<TNode> action)
            => Configure(action) as IOverridenTypedActionBuilder<TNode>;

        ITypedActionBuilder<TNode> INodeOptions<ITypedActionBuilder<TNode>>.SetOptions(NodeOptions nodeOptions)
            => SetOptions(nodeOptions) as ITypedActionBuilder<TNode>;

        IOverridenTypedActionBuilder<TNode> INodeOptions<IOverridenTypedActionBuilder<TNode>>.UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater)
            => UpdateOptions(nodeOptionsUpdater) as IOverridenTypedActionBuilder<TNode>;

        ITypedActionBuilder<TNode> INodeOptions<ITypedActionBuilder<TNode>>.UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater)
            => UpdateOptions(nodeOptionsUpdater) as ITypedActionBuilder<TNode>;

        IOverridenTypedActionBuilder<TNode> INodeOptions<IOverridenTypedActionBuilder<TNode>>.SetOptions(NodeOptions nodeOptions)
            => SetOptions(nodeOptions) as IOverridenTypedActionBuilder<TNode>;
    }
    
    internal class TimeEventNodeBuilder<TNode>(Node node, BaseActivityBuilder activityBuilder) :
        NodeBuilder(node, activityBuilder),
        ITimeEventActionBuilder<TNode>,
        IOverridenTimeEventActionBuilder<TNode>
        where TNode : class, ITimeEventActionNode
    {
        public ITimeEventActionBuilder<TNode> Configure(Action<TNode> action)
        {
            Node.ConfigurationAction = o => action((TNode)o);

            return this;
        }
        
        ITimeEventActionBuilder<TNode> IObjectFlowBase<ITimeEventActionBuilder<TNode>>.AddFlow<TToken>(string targetNodeName,
            ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as ITimeEventActionBuilder<TNode>;

        ITimeEventActionBuilder<TNode> IControlFlowBase<ITimeEventActionBuilder<TNode>>.AddControlFlow(string targetNodeName,
            ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as ITimeEventActionBuilder<TNode>;

        ITimeEventActionBuilder<TNode> IExceptionHandlerBase<ITimeEventActionBuilder<TNode>>.AddExceptionHandler<TException>(
            ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as ITimeEventActionBuilder<TNode>;

        IOverridenTimeEventActionBuilder<TNode> IObjectFlowBase<IOverridenTimeEventActionBuilder<TNode>>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow(targetNodeName, buildAction) as IOverridenTimeEventActionBuilder<TNode>;

        IOverridenTimeEventActionBuilder<TNode> IControlFlowBase<IOverridenTimeEventActionBuilder<TNode>>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IOverridenTimeEventActionBuilder<TNode>;

        IOverridenTimeEventActionBuilder<TNode> IExceptionHandlerBase<IOverridenTimeEventActionBuilder<TNode>>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler(exceptionHandler) as IOverridenTimeEventActionBuilder<TNode>;

        IOverridenTimeEventActionBuilder<TNode> IElementBuilderBase<TNode, IOverridenTimeEventActionBuilder<TNode>>.Configure(Action<TNode> action)
            => Configure(action) as IOverridenTimeEventActionBuilder<TNode>;

        IOverridenTimeEventActionBuilder<TNode> IOverridenObjectFlowBase<IOverridenTimeEventActionBuilder<TNode>>.UseFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => UseFlow<TToken>(targetNodeName, buildAction) as IOverridenTimeEventActionBuilder<TNode>;

        IOverridenTimeEventActionBuilder<TNode> IOverridenControlFlowBase<IOverridenTimeEventActionBuilder<TNode>>.UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => UseControlFlow(targetNodeName, buildAction) as IOverridenTimeEventActionBuilder<TNode>;
    }
}