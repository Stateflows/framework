using System;
using Stateflows.Activities.Models;
using Stateflows.Activities.Engine;

namespace Stateflows.Activities.Context.Classes;

internal sealed class PinContext(Node node, RootContext context, NodeScope scope)
    : NodeContext(node, null, context, scope), IPinContext
{
    public Type TokenType => Node.PinTokenType!;
}
