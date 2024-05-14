using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Utils;

namespace Stateflows.Activities.Context.Classes
{
    internal class SourceNodeContext : NodeContext, ISourceNodeContext
    {
        internal readonly Guid ThreadId;

//        private IEnumerable<object> input = null;
//        public IEnumerable<object> Input
//#pragma warning disable S2365 // Properties should not make collection or array copies
//            => input ??= Context
//                .GetStreams(Node, ThreadId)
//                .SelectMany(stream => stream.Tokens)
//                .ToArray();
//#pragma warning restore S2365 // Properties should not make collection or array copies

        public SourceNodeContext(Node node, RootContext context, Guid threadId)
            : base(node, context)
        {
            ThreadId = threadId;
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
