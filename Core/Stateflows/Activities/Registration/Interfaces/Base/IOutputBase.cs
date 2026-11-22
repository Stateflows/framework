using System.Collections.Generic;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;
using Stateflows.Payloads;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds typed output pins to a nested activity graph.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.7.-Output-Pin" />
    public interface IOutputBase<out TReturn>
    {
        /// <summary>Adds a typed output pin, targeted with OutputPin&lt;TToken&gt; by default.</summary>
        /// <param name="name">Optional globally unique node name; defaults to OutputPin&lt;TToken&gt;.Name.</param>
        TReturn AddOutputPin<TToken>(string? name = null);

        /// <summary>Adds an output pin for payload tokens with the specified names.</summary>
        /// <param name="tokenNames">Payload token names admitted by the pin.</param>
        /// <param name="pinName">Optional globally unique node name.</param>
        TReturn AddPayloadOutputPin(IReadOnlyList<string> tokenNames, string? pinName = null)
            => PayloadFlowScope.With(tokenNames, nameof(tokenNames), () => AddOutputPin<Payload>(pinName));
    }
}
