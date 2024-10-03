using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common;

namespace Stateflows.Activities.Registration
{
    internal class NodeBuilder :
        IActionBuilder,
        IActionBuilderWithOptions,
        ITypedActionBuilder,
        IAcceptEventActionBuilder,
        ISendEventActionBuilder,
        IInitialBuilder,
        IInputBuilder,
        IMergeBuilder,
        IJoinBuilder,
        IForkBuilder,
        IDecisionBuilder,
        IDataStoreBuilder,
        IInternal
    {
        public Node Node { get; }

        public IServiceCollection Services { get; }

        public Graph Result => Node.Graph;

        public BaseActivityBuilder ActivityBuilder { get; }

        public NodeBuilder(Node node, BaseActivityBuilder activityBuilder, IServiceCollection services)
        {
            Node = node;
            ActivityBuilder = activityBuilder;
            Services = services;
        }

        public IActionBuilder AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null)
            => AddFlowInternal<ControlToken>(targetNodeName, false, b => buildAction?.Invoke(b as IControlFlowBuilder));

        public IActionBuilder AddElseControlFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null)
            => AddFlowInternal<ControlToken>(targetNodeName, true, b => buildAction?.Invoke(b as IElseControlFlowBuilder));

        public IActionBuilder AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
            => AddFlowInternal<TToken>(targetNodeName, false, buildAction);

        public IActionBuilder AddElseFlow<TToken>(string targetNodeName, ElseObjectFlowBuildAction<TToken> buildAction = null)
            => AddFlowInternal<TToken>(targetNodeName, true, b => buildAction?.Invoke(b as IElseObjectFlowBuilder<TToken>));

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

        public IActionBuilderWithOptions SetOptions(NodeOptions nodeOptions)
        {
            Node.Options = nodeOptions;

            return this;
        }

        IActionBuilderWithOptions IControlFlowBase<IActionBuilderWithOptions>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IActionBuilderWithOptions IObjectFlowBase<IActionBuilderWithOptions>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IInitialBuilder IControlFlowBase<IInitialBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IInitialBuilder;

        ITypedActionBuilder IObjectFlowBase<ITypedActionBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as ITypedActionBuilder;

        ITypedActionBuilder IControlFlowBase<ITypedActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as ITypedActionBuilder;

        public IActionBuilderWithOptions AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            where TException : Exception
        {
            var targetNodeName = $"{Node.Name}.{typeof(TException).FullName}.ExceptionHandler";

            AddFlow<TException>(targetNodeName);
            AddFlow<NodeReferenceToken>(targetNodeName);

            ActivityBuilder.AddNode(
                NodeType.ExceptionHandler,
                targetNodeName,
                (ActionDelegateAsync)(c =>
                {
                    var contextObj = c as ActionContext;
                    var nodeOfOrigin = contextObj.InputTokens.OfType<TokenHolder<NodeReferenceToken>>().FirstOrDefault()?.Payload?.Node;
                    var context = new ExceptionHandlerContext<TException>(contextObj, Node, nodeOfOrigin, contextObj.NodeScope);

                    exceptionHandler?.Invoke(context);

                    contextObj.OutputTokens.AddRange(context.OutputTokens);

                    return Task.CompletedTask;
                }),
                null,
                typeof(TException)
            );

            return this;
        }

        ITypedActionBuilder IExceptionHandlerBase<ITypedActionBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as ITypedActionBuilder;

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

        IAcceptEventActionBuilder IObjectFlowBase<IAcceptEventActionBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IAcceptEventActionBuilder;

        IAcceptEventActionBuilder IControlFlowBase<IAcceptEventActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IAcceptEventActionBuilder;

        IAcceptEventActionBuilder IExceptionHandlerBase<IAcceptEventActionBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IAcceptEventActionBuilder;

        ISendEventActionBuilder IControlFlowBase<ISendEventActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as ISendEventActionBuilder;

        IDecisionBuilder IDecisionFlowBase<IDecisionBuilder>.AddFlow(string targetNodeName, ControlFlowBuildAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IDecisionBuilder;

        IDecisionBuilder IElseDecisionFlowBase<IDecisionBuilder>.AddElseFlow(string targetNodeName, ElseControlFlowBuildAction buildAction)
            => AddElseControlFlow(targetNodeName, buildAction) as IDecisionBuilder;

        IDataStoreBuilder IObjectFlowBase<IDataStoreBuilder>.AddFlow<TToken>(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction)
            => AddFlow<TToken>(targetNodeName, buildAction) as IDataStoreBuilder;
    }
}