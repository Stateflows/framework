using System;
using System.Linq;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ExceptionHandlerContext<TException> : ActionContext, IExceptionHandlerContext<TException>
        where TException : Exception
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        public TException exception = null;
        public TException Exception
            => exception ??= InputTokens.OfType<TokenHolder<TException>>().First().Payload;

        public INodeContext NodeOfOrigin { get; set; }

        public ExceptionHandlerContext(ActionContext context, Node nodeOfOrigin)
            : base(context.Context, context.NodeScope, context.Node, context.InputTokens)
        {
            NodeOfOrigin = new NodeContext(nodeOfOrigin, context.Context);
        }
    }
}
