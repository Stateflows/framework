using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Models;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;

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
        
        public async Task<bool> HandleExceptionAsync(RootContext rootContext, Node nodeOfOrigin, TException exception)
        {
            if (exception == null)
            {
                return false;
            }
            
            var actionContext = new ActionContext(rootContext, nodeScope, node, new List<TokenHolder>() { exception.ToExceptionHolder() });
            var context = new ExceptionHandlerContext<TException>(actionContext, node, nodeOfOrigin, nodeScope);
            await exceptionHandlerDelegate(context);
            
            return true;
        }
        
        public Task<bool> OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception)
            => HandleExceptionAsync((context as IRootContext).Context, node, exception as TException);

        public Task<bool> OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception)
            => HandleExceptionAsync((context as IRootContext).Context, node, exception as TException);

        public Task<bool> OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception)
            => HandleExceptionAsync((context as IRootContext).Context, (context.CurrentNode as NodeContext).Node, exception as TException);

        public Task<bool> OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception)
            => HandleExceptionAsync((context as IRootContext).Context, (context.CurrentNode as NodeContext).Node, exception as TException);

        public Task<bool> OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception)
            => HandleExceptionAsync((context as IRootContext).Context, (context.CurrentNode as NodeContext).Node, exception as TException);

        public Task<bool> OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception)
            => HandleExceptionAsync((context as IRootContext).Context, (context.SourceNode as NodeContext).Node, exception as TException);

        public Task<bool> OnFlowTransformationExceptionAsync<TToken>(ITransformationContext<TToken> context, Exception exception)
            => HandleExceptionAsync((context as IRootContext).Context, (context.SourceNode as NodeContext).Node, exception as TException);
    }
}