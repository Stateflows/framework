using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActionContext : ActivityNodeContext, IActionContext<Token>, IActivityNodeInspectionContext
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
            where TToken : Token, new()
            => OutputRange(new TToken[] { token });

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new()
        {
            OutputTokens.AddRange(tokens);
        }

        public void OutputRangeAsGroup<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new()
            => Output(tokens.ToGroupToken());

        public void PassTokensOfTypeOn<TToken>()
            where TToken : Token, new()
            => OutputRange(InputTokens.OfType<TToken>());

        public void PassTokensOfTypeOnAsGroup<TToken>()
            where TToken : Token, new()
            => Output(InputTokens.OfType<TToken>().ToGroupToken());

        public void PassAllOn()
            => OutputRange(InputTokens);

        public IEnumerable<Token> Input => InputTokens;

        public IEnumerable<Token> Tokens { get; }
    }
}