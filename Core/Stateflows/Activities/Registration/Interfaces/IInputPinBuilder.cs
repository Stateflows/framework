using Stateflows.Activities.Registration.Interfaces.Base;

namespace Stateflows.Activities;

/// <summary>Configures object flows carrying the input pin's declared token type.</summary>
public interface IInputPinBuilder<out TToken> : IDecisionFlowBase<TToken, IInputPinBuilder<TToken>>;
