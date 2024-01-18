using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ExceptionHandlerContext<TException> : ActionContext, IExceptionHandlerContext<TException>
        where TException : Exception
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        public TException exception = null;
        public TException Exception
            => exception ??= InputTokens.OfType<ExceptionToken<Exception>>().First(t => t.Exception is TException).Exception as TException;

        public INodeContext NodeOfOrigin { get; set; }

        public ExceptionHandlerContext(ActionContext context, Node nodeOfOrigin)
            : base(context.Context, context.NodeScope, context.Node, context.InputTokens)
        {
            NodeOfOrigin = new NodeContext(nodeOfOrigin, context.Context);
        }
    }
}
