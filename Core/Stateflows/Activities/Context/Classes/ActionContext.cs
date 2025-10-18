using System.Linq;
using System.Collections.Generic;
using Stateflows.Utils;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActionContext : ActivityNodeContext, IActionContext<object>, IRootContext
    {
        public ActionContext(RootContext context, NodeScope nodeScope, Node node, IEnumerable<TokenHolder> inputTokens, IEnumerable<TokenHolder> selectionTokens = null)
            : base(context, nodeScope, node)
        {
            InputTokens.AddRange(inputTokens);

            if (selectionTokens != null)
            {
                Tokens = selectionTokens;
            }
        }

        public List<TokenHolder> InputTokens { get; protected set; } = [];

        public List<TokenHolder> OutputTokens { get; protected set; } = [];

        public void Output<TToken>(TToken token)
            => OutputRange([ token ]);

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
            => OutputTokens.AddRange(tokens.ToTokenHolders());

        public void PassTokensOfTypeOn<TToken>()
            => OutputTokens.AddRange(InputTokens.OfType<TokenHolder<TToken>>());

        public void PassAllTokensOn()
            => OutputTokens.AddRange(InputTokens);

        public IEnumerable<TToken> GetTokensOfType<TToken>()
            => InputTokens.OfType<TokenHolder<TToken>>().ToTokens().ToArray();

        public IEnumerable<object> GetAllTokens()
            => InputTokens.ToBoxedTokens().ToArray();

        public IEnumerable<object> Tokens { get; protected set; }
    }
}