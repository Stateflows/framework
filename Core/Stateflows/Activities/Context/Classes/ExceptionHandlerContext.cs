using System;
using System.Linq;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Engine;

namespace Stateflows.Activities.Context.Classes
{
    internal class ExceptionHandlerContext<TException> : ActionContext, IExceptionHandlerContext<TException>
        where TException : Exception
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        public TException exception = null;
        public TException Exception
        {
            get
            {
                exception ??=
                    InputTokens.OfType<ExceptionHolder<TException>>().FirstOrDefault()?.TypedException ??
                    InputTokens.OfType<ExceptionHolder>().FirstOrDefault()?.Exception as TException;

                return exception;
            }
        }

        public INodeContext ProtectedNode { get; set; }

        public INodeContext NodeOfOrigin { get; set; }

        public ExceptionHandlerContext(ActionContext context, Node protectedNode, Node nodeOfOrigin, NodeScope nodeScope)
            : base(context.Context, context.NodeScope, context.Node, context.InputTokens)
        {
            ProtectedNode = new NodeContext(protectedNode, null, context.Context, nodeScope);
            NodeOfOrigin = new NodeContext(nodeOfOrigin, null, context.Context, nodeScope);
        }
    }
}
