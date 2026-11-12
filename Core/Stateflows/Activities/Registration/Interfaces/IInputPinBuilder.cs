using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities;

/// <summary>Configures object flows carrying the input pin's declared token type.</summary>
public interface IInputPinBuilder<out TToken> : IDecisionFlowBase<TToken, IInputPinBuilder<TToken>>;

/// <summary>Configures typed flows on an inherited input pin.</summary>
public interface IOverridenInputPinBuilder<TToken> : IInputPinBuilder<TToken>
{
    /// <summary>Reconfigures an inherited flow carrying this pin's token type.</summary>
    IOverridenInputPinBuilder<TToken> UseFlow(string targetNodeName, ObjectFlowBuildAction<TToken> buildAction);
}
