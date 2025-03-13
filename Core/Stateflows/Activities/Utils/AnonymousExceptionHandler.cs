using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Utils
{
    internal class AnonymousExceptionHandler<TException> : IActivityExceptionHandler
        where TException : Exception
    {
        private readonly NodeScope nodeScope;
        private readonly Node node;
        private readonly ExceptionHandlerDelegateAsync<TException> exceptionHandlerDelegate;
        
        public AnonymousExceptionHandler(NodeScope nodeScope, Node node, ExceptionHandlerDelegateAsync<TException> exceptionHandlerDelegate)
        {
            this.exceptionHandlerDelegate = exceptionHandlerDelegate;
            this.nodeScope = nodeScope;
            this.node = node;
        }
        
        public bool HandleException(RootContext rootContext, Node nodeOfOrigin, TException exception)
        {
            if (exception == null)
            {
                return false;
            }
            
            var actionContext = new ActionContext(rootContext, nodeScope, node, new List<TokenHolder>() { exception.ToExceptionHolder() });
            var context = new ExceptionHandlerContext<TException>(actionContext, node, nodeOfOrigin, nodeScope);
            exceptionHandlerDelegate(context);
            
            return true;
        }
        
        public bool OnActivityInitializationException(IActivityInitializationContext context, Exception exception)
            => HandleException((context as IRootContext).Context, node, exception as TException);

        public bool OnActivityFinalizationException(IActivityFinalizationContext context, Exception exception)
            => HandleException((context as IRootContext).Context, node, exception as TException);

        public bool OnNodeInitializationException(IActivityNodeContext context, Exception exception)
            => HandleException((context as IRootContext).Context, (context.CurrentNode as NodeContext).Node, exception as TException);

        public bool OnNodeFinalizationException(IActivityNodeContext context, Exception exception)
            => HandleException((context as IRootContext).Context, (context.CurrentNode as NodeContext).Node, exception as TException);

        public bool OnNodeExecutionException(IActivityNodeContext context, Exception exception)
            => HandleException((context as IRootContext).Context, (context.CurrentNode as NodeContext).Node, exception as TException);

        public bool OnFlowGuardException<TToken>(IGuardContext<TToken> context, Exception exception)
            => HandleException((context as IRootContext).Context, (context.SourceNode as NodeContext).Node, exception as TException);

        public bool OnFlowTransformationException<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception)
            => HandleException((context as IRootContext).Context, (context.SourceNode as NodeContext).Node, exception as TException);
    }
}