using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Utils;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActionContext : ActivityNodeContext, IActionContext<object>, IActivityNodeInspectionContext
    {
        public ActionContext(BaseContext context, Node node, IEnumerable<object> inputTokens = null, IEnumerable<object> selectionTokens = null)
            : base(context, node)
        {
            if (inputTokens != null)
            {
                InputTokens.AddRange(inputTokens);
            }

            if (selectionTokens != null)
            {
                Tokens = selectionTokens;
            }
        }

        public ActionContext(RootContext context, NodeScope nodeScope, Node node, IEnumerable<object> inputTokens, IEnumerable<object> selectionTokens = null)
            : base(context, nodeScope, node)
        {
            InputTokens.AddRange(inputTokens);

            if (selectionTokens != null)
            {
                Tokens = selectionTokens;
            }
        }

        public List<object> InputTokens { get; } = new List<object>();

        public List<object> OutputTokens { get; } = new List<object>();

        public void Output<TToken>(TToken token)
            => OutputRange(new TToken[] { token });

        public void OutputRange<TToken>(IEnumerable<TToken> tokens)
        {
            OutputTokens.AddRange(tokens.Box());
        }

        public void PassTokensOfTypeOn<TToken>()
            => OutputRange(InputTokens.OfType<TToken>());

        public void PassAllOn()
            => OutputRange(InputTokens);

        public IEnumerable<object> Input => InputTokens;

        public IEnumerable<object> Tokens { get; }
    }
}