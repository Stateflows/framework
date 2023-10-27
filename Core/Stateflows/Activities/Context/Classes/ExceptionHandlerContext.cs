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
        public TException Exception => exception ??= Input.OfType<ExceptionToken<Exception>>().First(t => t.Exception is TException).Exception as TException;

        public INodeContext NodeOfOrigin { get; set; }

        public ExceptionHandlerContext(RootContext context, NodeScope nodeScope, Node node, Node nodeOfOrigin, IEnumerable<Token> inputTokens)
            : base(context, nodeScope, node, inputTokens)
        {
            NodeOfOrigin = new NodeContext(nodeOfOrigin, context);
        }
    }
}
