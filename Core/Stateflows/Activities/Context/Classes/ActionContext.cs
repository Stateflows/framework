using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Utils;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActionContext : ActivityNodeContext, IActionContext<object>, IActivityNodeInspectionContext
    {
        public ActionContext(RootContext context, NodeScope nodeScope, Node node, IEnumerable<Token> inputTokens, IEnumerable<Token> selectionTokens = null)
            : base(context, nodeScope, node)
        {
            InputTokens.AddRange(inputTokens);

            if (selectionTokens != null)
            {
                Tokens = selectionTokens;
            }
        }

        public List<Token> InputTokens { get; } = new List<Token>();

        public List<Token> OutputTokens { get; } = new List<Token>();

        public void Output<TToken>(TToken token)
            => OutputRange(new TToken[] { token });

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
            => OutputTokens.AddRange(tokens.ToTokens());

        public void PassTokensOfTypeOn<TToken>()
            => OutputTokens.AddRange(InputTokens.OfType<Token<TToken>>());

        public void PassAllTokensOn()
            => OutputTokens.AddRange(InputTokens);

        public IEnumerable<TToken> GetTokensOfType<TToken>()
            => InputTokens.OfType<Token<TToken>>().FromTokens().ToArray();

        public IEnumerable<object> GetAllTokens()
            => InputTokens.FromTokens().ToArray();

        public IEnumerable<object> Tokens { get; }
    }
}