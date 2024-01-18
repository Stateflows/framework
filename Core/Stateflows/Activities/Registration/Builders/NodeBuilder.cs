﻿using System;
using System.Collections.Generic;
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

        public IActionBuilder AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction = null)
            => AddDataFlowInternal<ControlToken>(targetNodeName, false, b => buildAction?.Invoke(b as IControlFlowBuilder));

        public IActionBuilder AddElseControlFlow(string targetNodeName, ElseControlFlowBuilderAction buildAction = null)
            => AddDataFlowInternal<ControlToken>(targetNodeName, true, b => buildAction?.Invoke(b as IElseControlFlowBuilder));

        public IActionBuilder AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            => AddDataFlowInternal<TToken>(targetNodeName, false, buildAction);

        public IActionBuilder AddElseTokenFlow<TToken>(string targetNodeName, ElseObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            => AddDataFlowInternal<TToken>(targetNodeName, true, b => buildAction?.Invoke(b as IElseObjectFlowBuilder<TToken>));

        public IActionBuilder AddDataFlowInternal<TToken>(string targetNodeName, bool isElse, ObjectFlowBuilderAction<TToken> buildAction = null)
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

        IActionBuilderWithOptions IControlFlow<IActionBuilderWithOptions>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IActionBuilderWithOptions IObjectFlow<IActionBuilderWithOptions>.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow(targetNodeName, buildAction) as IActionBuilderWithOptions;

        IInitialBuilder IControlFlow<IInitialBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IInitialBuilder;

        ITypedActionBuilder IObjectFlow<ITypedActionBuilder>.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow<TToken>(targetNodeName, buildAction) as ITypedActionBuilder;

        ITypedActionBuilder IControlFlow<ITypedActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as ITypedActionBuilder;

        public IActionBuilderWithOptions AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            where TException : Exception
        {
            var targetNodeName = $"{Node.Name}.{typeof(TException).FullName}.ExceptionHandler";

            AddTokenFlow<ExceptionToken<TException>>(targetNodeName);
            ActivityBuilder.AddNode(
                NodeType.ExceptionHandler,
                targetNodeName,
                (ActionDelegateAsync)(c =>
                {
                    var contextObj = c as ActionContext;
                    var context = new ExceptionHandlerContext<TException>(contextObj, Node);

                    exceptionHandler?.Invoke(context);

                    c.OutputRange((IEnumerable<Token>)context.OutputTokens);

                    return Task.CompletedTask;
                }),
                null,
                typeof(TException)
            );

            return this;
        }

        ITypedActionBuilder IExceptionHandler<ITypedActionBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as ITypedActionBuilder;

        IActionBuilder IExceptionHandler<IActionBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IActionBuilder;

        IInputBuilder IObjectFlow<IInputBuilder>.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow<TToken>(targetNodeName, buildAction) as IInputBuilder;

        void IObjectFlow.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow<TToken>(targetNodeName, buildAction);

        void IControlFlow.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction);

        IForkBuilder IObjectFlow<IForkBuilder>.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow<TToken>(targetNodeName, buildAction) as IForkBuilder;

        IForkBuilder IControlFlow<IForkBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IForkBuilder;

        IAcceptEventActionBuilder IObjectFlow<IAcceptEventActionBuilder>.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow<TToken>(targetNodeName, buildAction) as IAcceptEventActionBuilder;

        IAcceptEventActionBuilder IControlFlow<IAcceptEventActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IAcceptEventActionBuilder;

        IAcceptEventActionBuilder IExceptionHandler<IAcceptEventActionBuilder>.AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            => AddExceptionHandler<TException>(exceptionHandler) as IAcceptEventActionBuilder;

        ISendEventActionBuilder IControlFlow<ISendEventActionBuilder>.AddControlFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as ISendEventActionBuilder;

        IDecisionBuilder IDecisionFlow<IDecisionBuilder>.AddFlow(string targetNodeName, ControlFlowBuilderAction buildAction)
            => AddControlFlow(targetNodeName, buildAction) as IDecisionBuilder;

        IDecisionBuilder IElseDecisionFlow<IDecisionBuilder>.AddElseFlow(string targetNodeName, ElseControlFlowBuilderAction buildAction)
            => AddElseControlFlow(targetNodeName, buildAction) as IDecisionBuilder;

        IDataStoreBuilder IObjectFlow<IDataStoreBuilder>.AddTokenFlow<TToken>(string targetNodeName, ObjectFlowBuilderAction<TToken> buildAction)
            => AddTokenFlow<TToken>(targetNodeName, buildAction) as IDataStoreBuilder;
    }
}