using System.Linq;
using System.Collections.Generic;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActionContext : ActivityNodeContext, IActionContext, IActivityNodeInspectionContext
    {
        public ActionContext(BaseContext context, Node node, IEnumerable<Token> inputTokens = null)
            : base(context, node)
        {
            if (inputTokens != null)
            {
                Input.AddRange(inputTokens);
            }
        }

        public ActionContext(RootContext context, NodeScope nodeScope, Node node, IEnumerable<Token> inputTokens)
            : base(context, nodeScope, node)
        {
            Input.AddRange(inputTokens);
        }

        public List<Token> Input { get; } = new List<Token>();

        public List<Token> Output { get; } = new List<Token>();

        public void OutputToken<TToken>(TToken token)
            where TToken : Token, new()
            => OutputTokensRange(new TToken[] { token });

        public void OutputTokensRange<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new()
        {
            Output.AddRange(tokens);
        }

        public void OutputTokensRangeAsGroup<TToken>(IEnumerable<TToken> tokens)
            where TToken : Token, new()
            => OutputToken(tokens.ToGroupToken());

        public void PassTokensOfType<TToken>()
            where TToken : Token, new()
            => OutputTokensRange(Input.OfType<TToken>());

        public void PassTokensOfTypeAsGroup<TToken>()
            where TToken : Token, new()
            => OutputToken(Input.OfType<TToken>().ToGroupToken());

        public void PassAllTokens()
            => OutputTokensRange(Input);

        public IEnumerable<Token> InputTokens => Input;
    }
}