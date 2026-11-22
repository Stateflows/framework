using System.Collections.Generic;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;
using Stateflows.Payloads;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds typed input pins to an activity graph.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.6.-Input-Pin" />
    public interface IInputBase<out TReturn>
    {
        /// <summary>Adds a typed input pin with object flows of its declared token type.</summary>
        /// <remarks>The pin name is generated as the containing scope name followed by
        /// <c>InputPin&lt;TToken&gt;.Name</c>.</remarks>
        TReturn AddInputPin<TToken>(System.Action<IInputPinBuilder<TToken>> buildAction);

        /// <summary>Adds an input pin for payload tokens with the specified names.</summary>
        /// <param name="tokenNames">Payload token names admitted by the pin.</param>
        /// <param name="buildAction">Callback configuring the pin's outgoing flows.</param>
        TReturn AddPayloadInputPin(IReadOnlyList<string> tokenNames, System.Action<IInputPinBuilder<Payload>> buildAction)
            => PayloadFlowScope.With(tokenNames, nameof(tokenNames), () => AddInputPin<Payload>(buildAction));
    }
}
