using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class SourceNodeContext : NodeContext, ISourceNodeContext
    {
        internal readonly Guid ThreadId;

        private IEnumerable<Token> input = null;
        public IEnumerable<Token> InputTokens
#pragma warning disable S2365 // Properties should not make collection or array copies
            => input ??= Context
                .GetStreams(Node, ThreadId)
                .SelectMany(stream => stream.Tokens)
                .ToArray();
#pragma warning restore S2365 // Properties should not make collection or array copies

        public SourceNodeContext(Node node, RootContext context, Guid threadId)
            : base(node, context)
        {
            ThreadId = threadId;
        }
    }
}
