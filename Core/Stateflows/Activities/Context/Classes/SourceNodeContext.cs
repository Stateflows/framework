using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Utils;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class SourceNodeContext : NodeContext, ISourceNodeContext
    {
        internal readonly Guid ThreadId;

        public SourceNodeContext(Node node, RootContext context, NodeScope nodeScope)
            : base(node, null, context, nodeScope)
        {
            ThreadId = nodeScope.ThreadId;
        }

        public IEnumerable<TToken> GetTokensOfType<TToken>()
            => Context
                .GetNodeStreams(Node, ThreadId, true)
                .SelectMany(stream => stream.Tokens)
                .OfType<TokenHolder<TToken>>()
                .ToTokens()
                .ToArray();
    }
}
