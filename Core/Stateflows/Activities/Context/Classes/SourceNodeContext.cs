using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Utils;
using Stateflows.Activities.Engine;

namespace Stateflows.Activities.Context.Classes
{
    internal class SourceNodeContext : NodeContext, ISourceNodeContext
    {
        internal readonly Guid ThreadId;

        public SourceNodeContext(Node node, RootContext context, NodeScope nodeScope)
            : base(node, context, nodeScope)
        {
            ThreadId = nodeScope.ThreadId;
        }

        public IEnumerable<TToken> GetTokensOfType<TToken>()
            => Context
                .GetActivatedStreams(Node, ThreadId)
                .SelectMany(stream => stream.Tokens)
                .OfType<TokenHolder<TToken>>()
                .FromTokens()
                .ToArray();
    }
}
