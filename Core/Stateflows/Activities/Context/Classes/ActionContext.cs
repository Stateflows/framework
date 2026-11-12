using System.Linq;
using System.Collections.Generic;
using Stateflows.Utils;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActionContext : ActivityNodeContext, IActionContext<object>, IRootContext
    {
        public ActionContext(RootContext context, NodeScope nodeScope, Node node, IEnumerable<TokenHolder> inputTokens, IEnumerable<TokenHolder> selectionTokens = null)
            : base(context, nodeScope, node)
        {
            InputTokens.AddRange(inputTokens);
            isStreamed = Node.Type is not NodeType.AcceptEventAction and not NodeType.TimeEventAction &&
                InputTokens.Any(token => token.IsStreamed);

            var selection = selectionTokens ?? nodeScope.SelectionTokens;
            if (selection != null)
            {
                Tokens = selection.ToBoxedTokens().ToArray();
            }
        }

        public List<TokenHolder> InputTokens { get; protected set; } = [];

        public List<TokenHolder> OutputTokens { get; protected set; } = [];

        internal Stateflows.Activities.Streams.ActivityContinuationFrame ResumeFrame { get; set; }

        private bool isStreamed;
        public override ExecutionMode ExecutionMode
            => isStreamed || base.ExecutionMode == ExecutionMode.Streamed
                ? ExecutionMode.Streamed : ExecutionMode.Transactional;
        internal bool IsStreamed
        {
            get => ExecutionMode == ExecutionMode.Streamed;
            set => isStreamed = value;
        }

        public void Output<TToken>(TToken token)
            => OutputRange(new[] { token });

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
        {
            var holders = tokens.ToTokenHolders().ToArray();
            foreach (var holder in holders)
            {
                holder.IsStreamed = IsStreamed;
            }
            OutputTokens.AddRange(holders);
        }

        public void PassTokensOfTypeOn<TToken>()
            => OutputTokens.AddRange(InputTokens.OfTokenType<TToken>()
                .Select(token => token.CopyForExecution(IsStreamed || token.IsStreamed)));

        public void PassAllTokensOn()
            => OutputTokens.AddRange(InputTokens
                .Select(token => token.CopyForExecution(IsStreamed || token.IsStreamed)));

        public IEnumerable<TToken> GetTokensOfType<TToken>()
            => InputTokens.ToTokensOfType<TToken>().ToArray();
        
        public bool HasTokensOfType<TToken>()
            => InputTokens.OfType<TokenHolder<TToken>>().Any();

        public IEnumerable<object> GetAllTokens()
            => InputTokens.ToBoxedTokens().ToArray();

        public IEnumerable<object> Tokens { get; protected set; }
    }
}