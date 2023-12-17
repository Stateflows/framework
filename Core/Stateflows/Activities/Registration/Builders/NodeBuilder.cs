using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Common.Classes;

namespace Stateflows.Activities.Registration
{
    internal class NodeBuilder :
        IActionBuilder,
        IActionBuilderWithOptions,
        ITypedActionBuilder,
        IInitialBuilder,
        IInputBuilder,
        IMergeBuilder,
        IJoinBuilder,
        IForkBuilder,
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

        public IActionBuilder AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction = null)
            => AddObjectFlow<ControlToken>(targetNodeName, b => buildAction?.Invoke(b as IFlowBuilder));

        public IActionBuilder AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
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

        IActionBuilderWithOptions IControlFlow<IActionBuilderWithOptions>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IActionBuilderWithOptions IObjectFlow<IActionBuilderWithOptions>.AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction)
            => AddObjectFlow(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IInitialBuilder IControlFlow<IInitialBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IInitialBuilder;

        ITypedActionBuilder IObjectFlow<ITypedActionBuilder>.AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction)
            => AddObjectFlow<TToken>(targetNodeName, buildAction) as ITypedActionBuilder;

        ITypedActionBuilder IControlFlow<ITypedActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as ITypedActionBuilder;

        public IActionBuilderWithOptions AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            where TException : Exception
        {
            var targetNodeName = $"{Node.Name}.{typeof(TException).FullName}.ExceptionHandler";

            AddObjectFlow<ExceptionToken<TException>>(targetNodeName);
            ActivityBuilder.AddNode(
                NodeType.ExceptionHandler,
                targetNodeName,
                c =>
                {
                    var contextObj = c as ActionContext;
                    var context = new ExceptionHandlerContext<TException>(contextObj.Context, contextObj.NodeScope, contextObj.Node, Node, contextObj.Input);

                    exceptionHandler?.Invoke(context);

                    c.OutputRange(context.OutputTokens);

                    return Task.CompletedTask;
                },
                null,
                typeof(TException)
            );

            return this;
        }

        ITypedActionBuilder IExceptionHandler<ITypedActionBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as ITypedActionBuilder;

        IActionBuilder IExceptionHandler<IActionBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IActionBuilder;

        IInputBuilder IObjectFlow<IInputBuilder>.AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction)
            => AddObjectFlow<TToken>(targetNodeName, buildAction) as IInputBuilder;

        void IObjectFlow.AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction)
            => AddObjectFlow<TToken>(targetNodeName, buildAction);

        void IControlFlow.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction);

        IForkBuilder IObjectFlow<IForkBuilder>.AddObjectFlow<TToken>(string targetNodeName, FlowBuilderAction<TToken> buildAction)
            => AddObjectFlow<TToken>(targetNodeName, buildAction) as IForkBuilder;

        IForkBuilder IControlFlow<IForkBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IForkBuilder;
    }
}