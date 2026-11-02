using Stateflows.Activities.Models;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Registration.Builders;

internal sealed class InputPinBuilder<TToken>(Node node, BaseActivityBuilder activity)
    : NodeBuilder(node, activity), IInputPinBuilder<TToken>
{
    public IInputPinBuilder<TToken> AddFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
    {
        AddFlow<TToken>(targetNodeName, buildAction);
        return this;
    }
}
