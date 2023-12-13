using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActionContext : BaseContext, IActionContext, IActivityNodeContext, IActivityNodeInspectionContext
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        IActivityInspectionContext IActivityNodeInspectionContext.Activity => Activity;

        public CancellationToken CancellationToken => Context.Executor.GetCancellationToken(Node);

        public Node Node { get; set; }

        public ActionContext(RootContext context, NodeScope nodeScope, Node node, IEnumerable<Token> inputTokens)
            : base(context, nodeScope)
        {
            Node = node;
            InputTokens.AddRange(inputTokens);
        }

        private INodeContext currentNode = null;
        public INodeContext CurrentNode => currentNode ??= new NodeContext(Node, Context);

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

        public void PassOfType<TToken>()
            where TToken : Token, new()
            => OutputRange(Input.OfType<TToken>());

        public void PassAll()
            => OutputRange(Input);

        public IEnumerable<Token> Input => InputTokens;
    }
}
