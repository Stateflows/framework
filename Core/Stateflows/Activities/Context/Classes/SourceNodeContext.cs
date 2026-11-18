using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Utils;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Payloads;

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
                .ToTokensOfType<TToken>()
                .ToArray();
        
        public bool HasTokensOfType<TToken>()
            => Context
                .GetNodeStreams(Node, ThreadId, true)
                .SelectMany(stream => stream.Tokens)
                .OfType<TokenHolder<TToken>>()
                .Any();

        public IEnumerable<Payload> GetPayloadTokens(string tokenName)
            => GetPayloadTokens([tokenName]);

        public IEnumerable<Payload> GetPayloadTokens(IReadOnlyList<string> tokenNames)
        {
            var names = PayloadNameSet.Validate(tokenNames, nameof(tokenNames));
            return Context
                .GetNodeStreams(Node, ThreadId, true)
                .SelectMany(stream => stream.Tokens)
                .Where(token => token.BoxedPayload is Payload payload && names.Contains(payload.Name, StringComparer.Ordinal))
                .Select(token => (Payload)token.BoxedPayload)
                .ToArray();
        }

        public bool HasPayloadTokens(string tokenName)
            => HasPayloadTokens([tokenName]);

        public bool HasPayloadTokens(IReadOnlyList<string> tokenNames)
            => GetPayloadTokens(tokenNames).Any();
    }
}
